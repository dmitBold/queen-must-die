using Core;
using Unity.Cinemachine;
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
        public bool IsActive => _isInitialized && _controller != null && _controller.isActive;

        private readonly ScenesManager _scenesManager;
        private string _previousSceneName;
        private AssemblyController _controller;
        private bool _isInitialized; // Флаг, что сцена загружена и контроллер инициализирован
        private System.Action _pendingOnComplete;

        public AssemblyService(ScenesManager scenesManager, CinemachineBrain cinemachineBrain)
        {
            _scenesManager = scenesManager;
        }

        public void OpenAssembly(InteractableView viewPrefab, System.Action onCompleteCallback = null)
        {
            _previousSceneName = SceneManager.GetActiveScene().name;

            if (_isInitialized && _controller != null)
            {
                _scenesManager.SetActive(SceneNames.Assembly);
                _controller.InitializeAssembly(viewPrefab);
                ActivateAssemblyMode(onCompleteCallback);
                return;
            }

            _scenesManager.LoadAdditive(SceneNames.Assembly, scene =>
            {
                _controller = FindController(scene);

                if (_controller == null)
                {
                    Debug.LogError($"[AssemblyService] AssemblyController not found in scene '{SceneNames.Assembly}'.");
                    return;
                }

                _controller.InitializeAssembly(viewPrefab);
                _isInitialized = true;

                _scenesManager.SetActive(SceneNames.Assembly);
                ActivateAssemblyMode(onCompleteCallback);
            });
        }

        private void ActivateAssemblyMode(System.Action onCompleteCallback)
        {
            _controller.OnCompleted.RemoveListener(OnControllerCompleted);
            _pendingOnComplete = onCompleteCallback;
            _controller.OnCompleted.AddListener(OnControllerCompleted);

            _controller.EnterAssembly();
        }

        private void OnControllerCompleted()
        {
            _pendingOnComplete?.Invoke();
            CloseAssembly();
        }

        public void CloseAssembly()
        {
            if (_controller != null)
            {
                _controller.ExitAssembly();
            }

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