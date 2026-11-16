using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core;
using Zenject;

namespace NightCycle
{
    public class LevelLoader : MonoBehaviour
    {
        [SerializeField] private GameScene targetScene;
        
        private SceneTransitionService _transitionService;

        [Header("Куда спавнить игрока в новой сцене")]
        [SerializeField] private Transform exitSpawnPoint;

        // либо явные координаты:
        [SerializeField] private Vector3 customSpawnPosition;
        [SerializeField] private Vector3 customSpawnEuler;
        [SerializeField] private bool useCustomSpawn;


        [Inject] private IFlashlightProvider _flashlightProvider;

        [Inject]
        private void Construct(SceneTransitionService transitionService)
        {
            _transitionService = transitionService;
        }
        public Animator transition;

        // Триггер затемнения в CrossFade.controller. Проявление в новой сцене играет состояние по умолчанию её LevelLoader.
        private const string FadeOutTrigger = "Start";

        /// <summary>Сколько ждать затемнения CrossFade перед загрузкой, секунды. Совпадает с длиной клипа CrossFadeStart.</summary>
        [SerializeField, Min(0f)] private float _fadeOutDuration = 1f;

        private bool _isFadingOut;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                LoadNext();
            }
        }

        /// <summary>Затемняет экран через CrossFade и переходит в targetScene через сцену загрузки.</summary>
        public void LoadNext()
        {
            if (_isFadingOut || _transitionService.IsTransitioning)
                return;

            if (transition == null)
            {
                BeginTransition();
                return;
            }

            StartCoroutine(FadeOutAndLoad());
        }

        private IEnumerator FadeOutAndLoad()
        {
            _isFadingOut = true;
            transition.SetTrigger(FadeOutTrigger);
            yield return new WaitForSeconds(_fadeOutDuration);
            _isFadingOut = false;
            BeginTransition();
        }

        private void BeginTransition()
        {
            if (!_transitionService.TryBegin(SceneNames.GetName(targetScene)))
                return;

            var fl = _flashlightProvider?.Current;
            if (fl != null)
                _flashlightProvider.PendingEssence = fl.GetEssense();

            if (useCustomSpawn)
            {
                if (exitSpawnPoint != null)
                    PendingSpawn.Set(exitSpawnPoint.position, exitSpawnPoint.rotation);
                else
                    PendingSpawn.Set(customSpawnPosition, Quaternion.Euler(customSpawnEuler));
            }
        }
    }
}
