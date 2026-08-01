using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Zenject;

namespace NightCycle
{
    public class CleanupManager : MonoBehaviour
    {
        [SerializeField] List<GameObject> stages;
        [SerializeField] List<AudioClip> musicTracks;
        private bool isMusicStarted = false;
        private int currentStageIndex = 0;

        //test
        public Light scene_light;
        public List<CandleController> OrderedCandles;
        public List<CandleController> ImmediateCandles;
        public float CandleTime;
        public float FirstDelay;
        public AudioClip candleSound;
        public AudioClip TinderSound;
        [SerializeField]
        private List<UnityEvent> _events = new List<UnityEvent>();
        // Изменено: теперь мы храним корутины для каждого источника света отдельно
        private Dictionary<Light, Coroutine> _lightCoroutines = new Dictionary<Light, Coroutine>();
        //test

        //test
        public AudioClip KnockSound;
        public GameObject door;
        public AudioClip Reznya;
        private AudioService _audioService;
        //test
        public UnityEvent ON_Cleanup_Completed;

        //Light
        public float turnOFF_time;
        public float turnON_time;
        public List<Light> AllLight;

        // Изменено: используем словарь для надежной привязки изначальной интенсивности к конкретному свету
        private Dictionary<Light, float> _originalIntensities = new Dictionary<Light, float>();
        //

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

            _audioService.musicSource.loop = true;

            int trackIndex = Mathf.Clamp(currentStageIndex, 0, musicTracks.Count - 1);
            AudioClip clipToPlay = musicTracks[trackIndex];

            if (_audioService.musicSource.clip != clipToPlay)
            {
                _audioService.musicSource.clip = clipToPlay;
                _audioService.musicSource.Play();
            }
        }

        public void StartCleanup()
        {
            _audioService.PlaySound(TinderSound);
            if (isMusicStarted) return;

            isMusicStarted = true;
        }

        public void AdvanceStage()
        {
            Debug.Log("120120120120");
            currentStageIndex++;
            UpdateMusic();
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

        IEnumerator CandlesRoutine()
        {
            yield return new WaitForSeconds(FirstDelay);

            foreach (CandleController candle in OrderedCandles)
            {
                yield return new WaitForSeconds(CandleTime);
                _audioService.PlaySound(candleSound);
                candle.SetCandleState(true);
            }
            TurnOnAllLight();
            foreach (CandleController candle in ImmediateCandles)
            {
                candle.SetCandleState(true);
            }
            _audioService.PlaySound(candleSound, 2.0f);
        }

        public void apply_stage_effect()
        {
            if (_events != null && currentStageIndex >= 0 && currentStageIndex < _events.Count)
            {
                _events[currentStageIndex]?.Invoke();
            }
            switch (currentStageIndex)
            {
                case 1:
                    TurnOffAllLight();
                    if (scene_light != null) scene_light.enabled = false;
                    StartCoroutine(CandlesRoutine());
                    break;
                case 2:
                    _audioService.PlaySoundAtPoint_loop(Reznya, new Vector3(-3.86f, 10.51f, -102.38f), 1, true, 0.27f, 4.29f);
                    _audioService.PlaySoundAtPoint_loop(Reznya, new Vector3(-9.71f, 10.51f, -120.86f), 1, true, 0.27f, 6.18f);
                    break;
                case 3:
                    ON_Cleanup_Completed?.Invoke();
                    _audioService.PlaySound(KnockSound);
                    break;
                default:
                    break;
            }
        }

        public void StopMusic()
        {
            _audioService.musicSource.Stop();
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
                ChangeLightIntensity(light, 0f, turnOFF_time);
            }
        }

        public void TurnOnAllLight()
        {
            foreach (Light light in AllLight)
            {
                if (light != null && _originalIntensities.TryGetValue(light, out float origIntensity))
                {
                    ChangeLightIntensity(light, origIntensity, turnON_time);
                }
            }
        }
    }
}