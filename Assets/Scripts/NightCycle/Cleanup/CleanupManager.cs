using System.Collections;
using System.Collections.Generic;
using Core;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Zenject;

namespace NightCycle
{
    public class CleanupManager : MonoBehaviour
    {
        [SerializeField] List<GameObject> stages;
        [SerializeField] List<EventReference> musicTracks;
        private bool isMusicStarted = false;
        private int currentStageIndex = 0;

        //test
        public Light scene_light;
        public List<CandleController> OrderedCandles;
        public List<CandleController> ImmediateCandles;
        [Tooltip("Свечи, которые горят до затемнения: их свет, свечение меша и огонь плавно гаснут вместе со светом и плавно возвращаются")]
        public List<CandleController> BlackoutCandles;
        [Tooltip("За сколько секунд разгорается каждая свеча из OrderedCandles")]
        public float CandleFadeTime = 0.8f;
        [Tooltip("Источники, которые после затемнения возвращаются медленнее остальных (в SlowRestoreFactor раз)")]
        public List<Light> SlowRestoreLights;
        public float SlowRestoreFactor = 1.5f;
        public float CandleTime;
        public float FirstDelay;
        public EventReference candleSound;
        public EventReference TinderSound;
        [SerializeField]
        private List<UnityEvent> _events = new List<UnityEvent>();
        // Изменено: теперь мы храним корутины для каждого источника света отдельно
        private Dictionary<Light, Coroutine> _lightCoroutines = new Dictionary<Light, Coroutine>();
        //test

        //test
        public EventReference KnockSound;
        public GameObject door;
        public EventReference Reznya;
        private AudioService _audioService;
        private EventInstance _musicInstance;
        private int _musicTrackIndex = -1;
        private readonly List<EventInstance> _reznyaInstances = new List<EventInstance>();
        //test
        public UnityEvent ON_Cleanup_Completed;
        [Tooltip("Свет после второго этапа полностью погас")]
        public UnityEvent BlackoutCompleted = new UnityEvent();
        [Tooltip("Свечи зажглись по очереди и весь свет полностью вернулся")]
        public UnityEvent LightsRestored = new UnityEvent();

        //Light
        public float turnOFF_time;
        public float turnON_time;
        public List<Light> AllLight;

        // Изменено: используем словарь для надежной привязки изначальной интенсивности к конкретному свету
        private Dictionary<Light, float> _originalIntensities = new Dictionary<Light, float>();
        //

        [SerializeField] MusicController musicController;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        private void Start()
        {
            // Сохраняем изначальную интенсивность при старте
            foreach (Light light in AllLight)
            {
                if (light != null)
                {
                    _originalIntensities[light] = light.intensity;
                }
            }

            foreach (CandleController candle in OrderedCandles)
            {
                if (candle != null)
                {
                    candle.SetCandleState(false);
                }
            }
            foreach (CandleController candle in ImmediateCandles)
            {
                if (candle != null)
                {
                    candle.SetCandleState(false);
                }
            }
        }

        public void UpdateMusic()
        {
            if (musicTracks.Count == 0) return;

            int trackIndex = Mathf.Clamp(currentStageIndex, 0, musicTracks.Count - 1);
            if (trackIndex == _musicTrackIndex) return;

            StopMusic();
            _musicTrackIndex = trackIndex;
            if (musicTracks[trackIndex].IsNull) return;

            // Зацикливание трека задаётся в самом FMOD-ивенте (loop region).
            _musicInstance = RuntimeManager.CreateInstance(musicTracks[trackIndex]);
            _musicInstance.start();
        }

        public void StartCleanup()
        {
            _audioService.PlayFMODEvent(TinderSound);
            //if (isMusicStarted) return;

            //isMusicStarted = true;
        }

        public void AdvanceStage()
        {
            Debug.Log("120120120120");
            currentStageIndex++;
            //UpdateMusic();
            apply_stage_effect();

            if (currentStageIndex < stages.Count)
            {
                Debug.Log($"Переход на стадию: {stages[currentStageIndex].name}");
            }
            else
            {
                Debug.Log("Уборка полностью завершена!");
            }
        }

        IEnumerator BlackoutRoutine()
        {
            yield return new WaitForSeconds(turnOFF_time);
            BlackoutCompleted.Invoke();
        }

        IEnumerator CandlesRoutine()
        {
            yield return new WaitForSeconds(FirstDelay);

            foreach (CandleController candle in OrderedCandles)
            {
                yield return new WaitForSeconds(CandleTime);
                _audioService.PlayFMODEvent(candleSound);
                if (candle != null) candle.FadeTo(1f, CandleFadeTime);
            }
            TurnOnAllLight();
            foreach (CandleController candle in ImmediateCandles)
            {
                if (candle != null) candle.FadeTo(1f, turnON_time);
            }
            foreach (CandleController candle in BlackoutCandles)
            {
                if (candle != null) candle.FadeTo(1f, turnON_time);
            }
            _audioService.PlayFMODEvent(candleSound);

            bool hasSlowLights = SlowRestoreLights != null && SlowRestoreLights.Count > 0;
            yield return new WaitForSeconds(hasSlowLights ? turnON_time * SlowRestoreFactor : turnON_time);
            LightsRestored.Invoke();
        }

        public void apply_stage_effect()
        {
            if (_events != null && currentStageIndex >= 0 && currentStageIndex < _events.Count)
            {
                _events[currentStageIndex]?.Invoke();
            }
            switch (currentStageIndex)
            {
                case 2:
                    if (musicController != null)
                    {
                        musicController.PlayTrack(1);
                    }
                    TurnOffAllLight();
                    foreach (CandleController candle in BlackoutCandles)
                    {
                        if (candle != null) candle.FadeTo(0f, turnOFF_time);
                    }
                    if (scene_light != null) scene_light.enabled = false;
                    StartCoroutine(BlackoutRoutine());
                    StartCoroutine(CandlesRoutine());
                    break;
                case 3:
                    if (musicController != null)
                    {
                        musicController.PlayTrack(2);
                    }
                    PlayReznyaLoops();
                    break;
                case 4:
                    //musicController.PlayTrack(3);
                    ON_Cleanup_Completed?.Invoke();
                    _audioService.PlayFMODEvent(KnockSound);
                    break;
                default:
                    break;
            }
        }

        private void PlayReznyaLoops()
        {
            PlayReznyaLoop(new Vector3(-3.86f, 10.51f, -102.38f), 0.27f, 4.29f);
            PlayReznyaLoop(new Vector3(-9.71f, 10.51f, -120.86f), 0.27f, 6.18f);
        }

        private void PlayReznyaLoop(Vector3 position, float minDistance, float maxDistance)
        {
            EventInstance instance = _audioService.PlayFMODEventAtPoint(Reznya, position, minDistance, maxDistance);
            if (instance.isValid()) _reznyaInstances.Add(instance);
        }

        // FMOD-экземпляры не привязаны к сцене: без явной остановки лупы и музыка играли бы после её выгрузки.
        private void OnDestroy()
        {
            StopMusic();
            foreach (EventInstance instance in _reznyaInstances)
            {
                instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                instance.release();
            }
            _reznyaInstances.Clear();
        }

        /// <summary>
        /// Мгновенно ставит состояние после завершённой уборки при загрузке сохранения: свет стадий, свечи, звуки резни.
        /// Без затемнения, стука и ON_Cleanup_Completed, иначе снова откроется дверь катсцены. Вызывать после Start всех объектов сцены.
        /// </summary>
        public void RestoreCompleted()
        {
            // Свет переключают события стадий 2 и 3 (день → ночь → огонь), берём их же, чтобы не дублировать ссылки сцены.
            for (int i = 2; i <= 3 && i < _events.Count; i++)
            {
                _events[i]?.Invoke();
            }
            if (scene_light != null) scene_light.enabled = false;
            foreach (CandleController candle in OrderedCandles)
            {
                if (candle != null) candle.SetCandleState(true);
            }
            foreach (CandleController candle in ImmediateCandles)
            {
                if (candle != null) candle.SetCandleState(true);
            }
            foreach (CandleController candle in BlackoutCandles)
            {
                if (candle != null) candle.SetCandleState(true);
            }
            PlayReznyaLoops();
            currentStageIndex = Mathf.Max(currentStageIndex, stages.Count + 1);
        }

        public void StopMusic()
        {
            if (!_musicInstance.isValid()) return;
            _musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _musicInstance.release();
            _musicInstance = default;
        }

        public void ChangeLightIntensity(Light light, float targetIntensity, float duration)
        {
            if (light == null) return;

            if (_lightCoroutines.TryGetValue(light, out Coroutine existingCoroutine) && existingCoroutine != null)
            {
                StopCoroutine(existingCoroutine);
            }

            _lightCoroutines[light] = StartCoroutine(ChangeLightIntensityRoutine(light, targetIntensity, duration));
        }

        private IEnumerator ChangeLightIntensityRoutine(Light light, float targetIntensity, float duration)
        {
            if (duration <= 0f)
            {
                light.intensity = targetIntensity;
                yield break;
            }

            float startIntensity = light.intensity;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
                light.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
                yield return null;
            }

            light.intensity = targetIntensity;
        }

        public void TurnOffAllLight()
        {
            foreach (Light light in AllLight)
            {
                if (light == null) continue;
                _originalIntensities[light] = light.intensity;
                ChangeLightIntensity(light, 0f, turnOFF_time);
            }
        }

        public void TurnOnAllLight()
        {
            foreach (Light light in AllLight)
            {
                if (light != null && _originalIntensities.TryGetValue(light, out float origIntensity))
                {
                    float duration = SlowRestoreLights != null && SlowRestoreLights.Contains(light) ? turnON_time * SlowRestoreFactor : turnON_time;
                    ChangeLightIntensity(light, origIntensity, duration);
                }
            }
        }
    }
}