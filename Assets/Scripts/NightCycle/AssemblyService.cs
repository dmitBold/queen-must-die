using System.Threading.Tasks;
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
        private readonly CinemachineBrain _brain;
        private readonly CinemachineBlendDefinition _defaultCameraMode;
        private string _previousSceneName;
        private AssemblyController _controller;
        private bool _isInitialized; // Флаг, что сцена загружена и контроллер инициализирован
        private System.Action _pendingOnComplete;

        public AssemblyService(ScenesManager scenesManager, CinemachineBrain brain)
        {
            _scenesManager = scenesManager;
            _brain = brain;
            _defaultCameraMode = _brain.DefaultBlend;
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
            HUDController.instance.SetCrosshairActivity(false);
            SetupCameraForEntry();
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
            SetupCameraForExit();

            if (_controller != null)
            {
                _controller.ExitAssembly();
            }

            if (!string.IsNullOrEmpty(_previousSceneName))
            {
                _scenesManager.SetActive(_previousSceneName);
            }

            HUDController.instance.SetCrosshairActivity(true);
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

        private void SetupCameraForEntry()
        {
            _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            _brain.OutputCamera.cullingMask = LayerMask.GetMask("Assembly");
            _brain.OutputCamera.clearFlags = CameraClearFlags.SolidColor;
            _brain.OutputCamera.backgroundColor = new Color(0.102f, 0.102f, 0.098f);
        }

        private async void SetupCameraForExit()
        {
            _brain.OutputCamera.cullingMask = -1;
            _brain.OutputCamera.clearFlags = CameraClearFlags.Skybox;
            await Task.Delay(100);
            _brain.DefaultBlend = _defaultCameraMode;
        }
    }
}