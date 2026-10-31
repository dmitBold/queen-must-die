using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core
{
    /// <summary>
    /// Управляет сценой загрузки: асинхронно грузит целевую сцену без активации, плавно заполняет прогресс-бар
    /// и разрешает активацию только после того, как бар дошёл до 100% и вышло минимальное время показа.
    /// Цель берёт из SceneTransitionService; если её нет, отправляет игрока в главное меню.
    /// </summary>
    public class LoadingScreenController : MonoBehaviour
    {
        // При allowSceneActivation = false Unity останавливает AsyncOperation.progress на 0.9 и не двигает дальше.
        private const float ActivationProgressThreshold = 0.9f;
        private const int OverlaySortingOrder = 32767;

        [Header("UI")]
        [Tooltip("Корневой Canvas сцены загрузки (Screen Space - Overlay).")]
        [SerializeField] private Canvas rootCanvas;

        [Tooltip("Image прогресс-бара: Image Type = Filled, Fill Method = Horizontal, Fill Origin = Left.")]
        [SerializeField] private Image progressFill;

        [Header("Тайминги")]
        [Tooltip("Минимальное время показа сцены загрузки в секундах, чтобы бар не мигал при быстрой загрузке.")]
        [SerializeField, Min(0f)] private float minDisplaySeconds = 1.5f;

        [Tooltip("Скорость заполнения бара в долях за секунду. Полное заполнение занимает не меньше 1 / значение секунд.")]
        [SerializeField, Min(0.01f)] private float fillSpeed = 1f;

        [Tooltip("Пауза на заполненном баре перед активацией целевой сцены, секунды.")]
        [SerializeField, Min(0f)] private float holdAtFullSeconds = 0.25f;

        private ScenesManager _scenesManager;
        private SceneTransitionService _transitionService;

        [Inject]
        private void Construct(ScenesManager scenesManager, SceneTransitionService transitionService)
        {
            _scenesManager = scenesManager;
            _transitionService = transitionService;
        }

        private void Awake()
        {
            // UI из ProjectContext (HUD, инвентарь, квесты, настройки) переживает смену сцены и лежит в своих Canvas.
            // Общего API для скрытия и восстановления их состояния нет, поэтому перекрываем их непрозрачным
            // оверлеем с максимальным sorting order, а не скрываем по одному.
            rootCanvas.sortingOrder = OverlaySortingOrder;
        }

        private IEnumerator Start()
        {
            progressFill.fillAmount = 0f;

            string targetScene = _transitionService.TargetSceneName;
            if (string.IsNullOrEmpty(targetScene))
            {
                Debug.LogError($"[LoadingScreen] Целевая сцена не задана, загружается '{SceneNames.MainMenu}'.");
                targetScene = SceneNames.MainMenu;
            }

            AsyncOperation operation = _scenesManager.BeginLoadSingle(targetScene);
            if (operation == null && targetScene != SceneNames.MainMenu)
            {
                Debug.LogError($"[LoadingScreen] Не удалось начать загрузку '{targetScene}', загружается '{SceneNames.MainMenu}'.");
                operation = _scenesManager.BeginLoadSingle(SceneNames.MainMenu);
            }

            if (operation == null)
            {
                Debug.LogError("[LoadingScreen] Не удалось начать загрузку ни целевой сцены, ни главного меню.");
                _transitionService.Complete();
                yield break;
            }

            float elapsed = 0f;
            float visualFill = 0f;

            while (operation.progress < ActivationProgressThreshold || visualFill < 1f || elapsed < minDisplaySeconds)
            {
                // unscaledDeltaTime, потому что Time.timeScale может быть 0 (пауза, фокус-режим) и бар замер бы.
                float deltaTime = Time.unscaledDeltaTime;
                elapsed += deltaTime;

                float loadRatio = Mathf.Clamp01(operation.progress / ActivationProgressThreshold);
                float timeRatio = minDisplaySeconds > 0f ? Mathf.Clamp01(elapsed / minDisplaySeconds) : 1f;

                // Минимум из двух: бар не обгоняет реальную загрузку и не заполняется быстрее минимального времени.
                // MoveTowards сглаживает рывки AsyncOperation.progress и доводит бар до 1.0 после остановки на 0.9.
                float targetFill = Mathf.Min(loadRatio, timeRatio);
                visualFill = Mathf.MoveTowards(visualFill, targetFill, fillSpeed * deltaTime);
                progressFill.fillAmount = visualFill;

                yield return null;
            }

            yield return new WaitForSecondsRealtime(holdAtFullSeconds);

            // Флаг сбрасываем до активации: после неё эта сцена и корутина уничтожаются.
            _transitionService.Complete();
            operation.allowSceneActivation = true;
        }
    }
}
