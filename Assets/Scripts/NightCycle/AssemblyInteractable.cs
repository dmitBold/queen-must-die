using System.Threading.Tasks;
using Unity.Cinemachine;
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

        private CinemachineBrain _brain;
        private AssemblyService _assemblyService;

        [Inject]
        public void Construct(AssemblyService assemblyService, CinemachineBrain brain)
        {
            _assemblyService = assemblyService;
            _brain = brain;
        }

        public void Interact()
        {
            if (_assemblyService.IsActive)
            {
                _assemblyService.CloseAssembly();
                SetupCameraForExit();
            }
            else
            {
                SetupCameraForEntry();
                _assemblyService.OpenAssembly(viewPrefab, OnAssemblyCompleted);
            }
        }

        private void SetupCameraForEntry()
        {
            _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            _brain.OutputCamera.cullingMask = LayerMask.GetMask("Assembly");
        }

        private void SetupCameraForExit()
        {
            _brain.OutputCamera.cullingMask = -1;
        }

        private void OnAssemblyCompleted()
        {
            // Гарантируем восстановление настроек камеры при завершении сборки
            onAssemblyCompleted?.Invoke();
            SetupCameraForExit();
        }
    }
}