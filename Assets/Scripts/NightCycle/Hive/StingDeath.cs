using System.Collections;
using Core;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Смерть от роя: Бет замирает, камеру дёргает от укусов и заваливает набок, затем грузится последнее сохранение.
    /// Если сохранения нет, текущая сцена перезапускается через экран загрузки.
    /// </summary>
    public class StingDeath : MonoBehaviour
    {
        /// <summary>Сколько длится сцена смерти до загрузки, сек.</summary>
        [SerializeField] private float _duration = 2.5f;
        /// <summary>Размах рывков камеры от укусов, градусы.</summary>
        [SerializeField] private float _twitchAngle = 10f;
        /// <summary>Частота рывков камеры.</summary>
        [SerializeField] private float _twitchFrequency = 9f;
        /// <summary>Насколько камера опускается и заваливается набок к концу сцены, градусы.</summary>
        [SerializeField] private float _slumpPitch = 35f;
        [SerializeField] private float _slumpRoll = 30f;

        private UIManager _uiManager;
        private FirstPersonController _player;
        private SaveManager _saveManager;
        private SceneTransitionService _transitionService;

        /// <summary>Идёт ли сцена смерти.</summary>
        public bool IsPlaying { get; private set; }

        [Inject]
        private void Construct(UIManager uiManager, FirstPersonController player, SaveManager saveManager, SceneTransitionService transitionService)
        {
            _uiManager = uiManager;
            _player = player;
            _saveManager = saveManager;
            _transitionService = transitionService;
        }

        /// <summary>Запускает смерть. Повторный вызов ничего не делает.</summary>
        public void Play()
        {
            if (IsPlaying) return;
            IsPlaying = true;
            StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            // Катсцена ставит Frozen: контроллер перестаёт крутить камеру, и ею можно управлять отсюда.
            _uiManager.BeginCutscene();

            Transform view = _player.View;
            Quaternion start = view.localRotation;
            float seed = Random.value * 100f;

            for (float time = 0f; time < _duration; time += Time.deltaTime)
            {
                float k = time / _duration;
                float t = time * _twitchFrequency;
                // Рывки слабеют к концу: Бет перестаёт отбиваться.
                float twitch = _twitchAngle * (1f - k * 0.7f);
                Quaternion shake = Quaternion.Euler(
                    (Mathf.PerlinNoise(seed, t) - 0.5f) * 2f * twitch,
                    (Mathf.PerlinNoise(seed + 10f, t) - 0.5f) * 2f * twitch,
                    (Mathf.PerlinNoise(seed + 20f, t) - 0.5f) * 2f * twitch);
                float slump = Mathf.SmoothStep(0f, 1f, k);
                view.localRotation = start * Quaternion.Euler(_slumpPitch * slump, 0f, _slumpRoll * slump) * shake;
                yield return null;
            }

            if (!_saveManager.LoadGame())
                _transitionService.TryBegin(SceneManager.GetActiveScene().name);
        }
    }
}
