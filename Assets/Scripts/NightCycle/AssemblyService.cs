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
        public bool IsActive => _isInitialized && _controller != null && _controller.isActive;
        
        private readonly ScenesManager _scenesManager;
        private string _previousSceneName;
        private AssemblyController _controller;
        private bool _isInitialized; // Флаг, что сцена загружена и контроллер инициализирован

        public AssemblyService(ScenesManager scenesManager)
        {
            _scenesManager = scenesManager;
        }

        public void OpenAssembly(AssemblyView viewPrefab, System.Action onCompleteCallback = null)
        {
            _previousSceneName = SceneManager.GetActiveScene().name;

            // Если сцена уже загружена и инициализирована - просто активируем Enter
            if (_isInitialized && _controller != null)
            {
                ActivateAssemblyMode(onCompleteCallback);
                return;
            }

            // Иначе загружаем сцену и инициализируем
            _scenesManager.LoadAdditive(SceneNames.Assembly, scene =>
            {
                _controller = FindController(scene);

                if (_controller == null)
                {
                    Debug.LogError($"[AssemblyService] AssemblyController not found in scene '{SceneNames.Assembly}'.");
                    return;
                }

                // Инициализация на старте (создание вью, настройка сокетов)
                _controller.InitializeAssembly(viewPrefab);
                _isInitialized = true;
                
                _scenesManager.SetActive(SceneNames.Assembly);
                
                // После инициализации активируем режим сборки
                ActivateAssemblyMode(onCompleteCallback);
            });
        }
        
        private void ActivateAssemblyMode(System.Action onCompleteCallback)
        {
            if (_controller == null) return;
            
            // Подписываемся на завершение
            _controller.OnCompleted.AddListener(() =>
            {
                onCompleteCallback?.Invoke();
                CloseAssembly();
            });
            
            // Активируем режим сборки (Enter)
            _controller.OnEnterFocus();
        }

        public void CloseAssembly()
        {
            if (_controller != null)
            {
                _controller.ExitAssembly();
            }
            
            _controller = null;
            _isInitialized = false;

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