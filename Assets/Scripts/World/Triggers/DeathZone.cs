using Core;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Триггер смерти (например, дно провала): когда Бет входит в него, она замирает, экран темнеет
    /// и грузится последнее сохранение (чекпоинт). Без сохранения текущая сцена перезапускается.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DeathZone : MonoBehaviour
    {
        /// <summary>Длительность затемнения перед загрузкой, сек.</summary>
        [SerializeField, Min(0f)] private float _fadeDuration = 0.5f;

        private UIManager _uiManager;
        private SaveManager _saveManager;
        private SceneTransitionService _transitionService;
        private FirstPersonController _player;
        private bool _dead;

        [Inject]
        private void Construct(UIManager uiManager, SaveManager saveManager, SceneTransitionService transitionService,
            FirstPersonController player)
        {
            _uiManager = uiManager;
            _saveManager = saveManager;
            _transitionService = transitionService;
            _player = player;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_dead || other.GetComponentInParent<FirstPersonController>() != _player) return;

            _dead = true;
            _uiManager.BeginCutscene();

            if (FadeController.Instance != null)
                FadeController.Instance.FadeToBlack(_fadeDuration, LoadCheckpoint);
            else
                LoadCheckpoint();
        }

        private void LoadCheckpoint()
        {
            if (!_saveManager.LoadGame())
                _transitionService.TryBegin(SceneManager.GetActiveScene().name);
        }
    }
}
