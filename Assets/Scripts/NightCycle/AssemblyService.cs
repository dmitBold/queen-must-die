using Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NightCycle
{
    /// <summary>
    /// Координирует открытие и закрытие режима сборки:
    /// загружает сцену сборки аддитивно, извлекает AssemblyController,
    /// передаёт в него вью и делает сцену активной.
    /// </summary>
    public class AssemblyService
    {
        public AssemblyController controller => _controller;

        private readonly ScenesManager _scenesManager;
        private string _previousSceneName;
        private AssemblyController _controller;

        public AssemblyService(ScenesManager scenesManager)
        {
            _scenesManager = scenesManager;
        }

        public void OpenAssembly(AssemblyView viewPrefab, System.Action onCompleteCallback = null)
        {
            _previousSceneName = SceneManager.GetActiveScene().name;

            _scenesManager.LoadAdditive(SceneNames.Assembly, scene =>
            {
                _controller = FindController(scene);

                if (_controller == null)
                {
                    Debug.LogError($"[AssemblyService] AssemblyController not found in scene '{SceneNames.Assembly}'.");
                    return;
                }

                _scenesManager.SetActive(SceneNames.Assembly);

                // Подписываемся на завершение через лямбду, чтобы вызвать и коллбэк, и закрытие.
                _controller.OnCompleted.AddListener(() =>
                {
                    onCompleteCallback?.Invoke();
                    CloseAssembly();
                });

                _controller.InitializeAssembly(viewPrefab);
            });
        }

        public void CloseAssembly()
        {
            _controller.ExitAssembly();
            _controller = null;

            if (!string.IsNullOrEmpty(_previousSceneName))
            {
                _scenesManager.SetActive(_previousSceneName);
            }
        }

        private static AssemblyController FindController(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var controller = root.GetComponentInChildren<AssemblyController>(true);
                if (controller != null)
                    return controller;
            }
            return null;
        }
    }
}
