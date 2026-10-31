using UnityEngine;

namespace Core
{
    /// <summary>
    /// Хранит состояние перехода между сценами через сцену загрузки: целевую сцену и флаг «переход идёт».
    /// Единственная точка запуска перехода, поэтому именно здесь отсекаются повторные вызовы.
    /// </summary>
    public class SceneTransitionService
    {
        private readonly ScenesManager _scenesManager;

        public SceneTransitionService(ScenesManager scenesManager)
        {
            _scenesManager = scenesManager;
        }

        /// <summary>
        /// Истина с момента запуска перехода до начала активации целевой сцены.
        /// </summary>
        public bool IsTransitioning { get; private set; }

        /// <summary>
        /// Имя сцены, в которую идёт переход. Читается сценой загрузки; пусто, если переход не запускался.
        /// </summary>
        public string TargetSceneName { get; private set; }

        /// <summary>
        /// Запускает переход в целевую сцену через сцену загрузки.
        /// Возвращает false, если переход уже идёт или имя сцены пустое. В этом случае вызывающий код
        /// не должен менять состояние игры, потому что загрузка не начата.
        /// </summary>
        public bool TryBegin(string targetSceneName)
        {
            if (IsTransitioning)
                return false;

            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogError("[SceneTransitionService] Имя целевой сцены пусто, переход не запущен.");
                return false;
            }

            // Без сцены загрузки в Build Settings LoadSceneAsync вернёт null, и флаг навсегда остался бы взведённым.
            // Грузим напрямую, чтобы не остановить игру из-за ошибки настройки проекта.
            if (!Application.CanStreamedLevelBeLoaded(SceneNames.Loading))
            {
                Debug.LogError($"[SceneTransitionService] Сцена '{SceneNames.Loading}' не добавлена в Build Settings. Загрузка '{targetSceneName}' выполнена напрямую.");
                _scenesManager.LoadSingle(targetSceneName);
                return true;
            }

            IsTransitioning = true;
            TargetSceneName = targetSceneName;
            _scenesManager.LoadSingle(SceneNames.Loading);
            return true;
        }

        /// <summary>
        /// Сбрасывает состояние перехода. Вызывается сценой загрузки перед активацией целевой сцены,
        /// потому что после активации сама сцена загрузки уже уничтожена.
        /// </summary>
        public void Complete()
        {
            IsTransitioning = false;
            TargetSceneName = null;
        }
    }
}
