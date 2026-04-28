using UnityEngine;
using Zenject;
using UnityEngine.Events;

namespace NightCycle
{
    /// <summary>
    /// Ставится на интерактивный объект-деталь в игровой сцене.
    /// При взаимодействии загружает сцену сборки и передаёт в контроллер нужную вью.
    /// </summary>
    public class AssemblyInteractable : MonoBehaviour
    {
        public UnityEvent onAssemblyCompleted;
        [SerializeField] private InteractableView viewPrefab;

        private AssemblyService _assemblyService;

        [Inject]
        public void Construct(AssemblyService assemblyService)
        {
            _assemblyService = assemblyService;
        }

        public void Interact()
        {
            if (_assemblyService.IsActive)
            {
                _assemblyService.CloseAssembly();
            }
            else
            {
                _assemblyService.OpenAssembly(viewPrefab, OnAssemblyCompleted);
            }
        }

        private void OnAssemblyCompleted()
        {
            onAssemblyCompleted?.Invoke();
        }
    }
}