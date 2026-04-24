using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using Unity.Cinemachine;

namespace NightCycle
{
    public class AssemblyController : MonoBehaviour, IDisposable
    {
        [Header("References")] [SerializeField]
        private CinemachineCamera assemblyCamera;

        [SerializeField] private TextMeshProUGUI _hintText;

        [SerializeField] private Canvas assemblyCanvas;
        [SerializeField] private Transform spawnPoint;
        [Inject] private InventoryUI inventoryUI;

        [Header("Rotation")] [SerializeField] private float rotationSpeed = 80f;

        [Header("Sockets")] [SerializeField] private LayerMask socketLayer;

        public UnityEvent OnCompleted;
        public bool isActive;

        private InteractableView currentView;
        private Camera camera;
        private AssemblyView currentAssemblyView;
        private LockView currentLockView;
        private IReadOnlyList<AssemblySocket> activeSockets;
        private AssemblySocket currentSocket;
        private PlayerStateController playerStateController;
        private DiContainer container;
        private InteractableView _subscribedView;

        private readonly Dictionary<InteractableView, InteractableView> _prefabInstances = new();

        [Inject]
        private void Construct(PlayerStateController playerStateController, DiContainer container)
        {
            this.playerStateController = playerStateController;
            this.container = container;
        }

        private void Awake()
        {
            camera = Camera.main;

            if (assemblyCamera)
                assemblyCamera.gameObject.SetActive(false);

            inventoryUI.OnSocketFilled += OnAssemblyCompleted;
        }

        private void Update()
        {
            if (!isActive)
                return;

            // Обновляем состояние объекта в фокусе
            currentView?.UpdateFocusState();

            // Обрабатываем только если это сборочный объект
            if (currentAssemblyView != null)
            {
                HandleSocketHover();
                HandleSocketClick();
            }

            HandleRotation();
        }

        private void OnDestroy()
        {
            inventoryUI.OnSocketFilled -= OnAssemblyCompleted;
            if (_subscribedView != null)
                _subscribedView.onInteractionCompleted.RemoveListener(HandleViewInteractionCompleted);
        }

        public void Dispose()
        {
            inventoryUI.OnSocketFilled -= OnAssemblyCompleted;
            activeSockets = null;
            foreach (var instance in _prefabInstances.Values)
            {
                if (instance != null)
                    Destroy(instance.gameObject);
            }

            _prefabInstances.Clear();
            if (_subscribedView != null)
                _subscribedView.onInteractionCompleted.RemoveListener(HandleViewInteractionCompleted);
            currentView = null;
            _subscribedView = null;
        }

        /// <summary>
        /// Принимает префаб интерактивного объекта, спавнит его и запускает режим взаимодействия.
        /// </summary>
        public void InitializeAssembly(InteractableView viewPrefab)
        {
            if (currentView != null)
                currentView.gameObject.SetActive(false);

            if (_subscribedView != null)
            {
                _subscribedView.onInteractionCompleted.RemoveListener(HandleViewInteractionCompleted);
                _subscribedView = null;
            }

            if (!_prefabInstances.TryGetValue(viewPrefab, out var viewInstance))
            {
                viewInstance = container.InstantiatePrefabForComponent<InteractableView>(viewPrefab, spawnPoint.transform);
                _prefabInstances[viewPrefab] = viewInstance;
            }

            viewInstance.gameObject.SetActive(true);
            currentView = viewInstance;
            _subscribedView = viewInstance;
            _subscribedView.onInteractionCompleted.AddListener(HandleViewInteractionCompleted);
            SetHintText(currentView.HintText);

            // Определяем тип объекта и настраиваем соответствующим образом
            if (currentView is AssemblyView assemblyView)
            {
                currentAssemblyView = assemblyView;
                activeSockets = currentAssemblyView.Sockets;
                currentLockView = null;
            }
            else if (currentView is LockView lockView)
            {
                currentLockView = lockView;
                currentAssemblyView = null;
                activeSockets = null; // У замков нет сокетов
            }
        }

        public void EnterAssembly()
        {
            isActive = true;

            assemblyCanvas.gameObject.SetActive(true);
            HUDController.instance.DisableInteractionText();

            // Вызываем OnEnterFocus для текущего объекта
            currentView?.OnEnterFocus();

            // Настраиваем режим в зависимости от типа объекта
            if (currentAssemblyView != null)
            {
                inventoryUI.SetMode(InventoryUI.InventoryMode.AssemblyItemSelection);
            }
            else if (currentLockView != null)
            {
                // Для замков не нужен режим выбора предметов из инвентаря
                inventoryUI.SetMode(InventoryUI.InventoryMode.LockInteraction);
            }

            playerStateController.SetMode(PlayerMode.Focused);

            assemblyCamera.gameObject.SetActive(true);
            assemblyCamera.Priority = 100;
        }

        public void ExitAssembly()
        {
            // Вызываем OnExitFocus для текущего объекта
            currentView?.OnExitFocus();

            assemblyCanvas.gameObject.SetActive(false);
            isActive = false;

            if (currentAssemblyView != null)
            {
                inventoryUI.ExitSelection();
            }
            else if (currentLockView != null)
            {
                inventoryUI.ExitSelection();
            }

            playerStateController.SetMode(PlayerMode.FreeMovement);

            assemblyCamera.Priority = 0;
            assemblyCamera.gameObject.SetActive(false);
        }

        public void SetHintText(string text)
        {
            if (!string.IsNullOrEmpty(text))
                _hintText.text = text;
        }

        private void OnAssemblyCompleted()
        {
            // Проверяем завершение только для сборочных объектов
            if (currentAssemblyView != null && !currentAssemblyView.CheckSocketsCompletion())
                return;
        }

        private void HandleViewInteractionCompleted()
        {
            OnCompleted?.Invoke();
        }

        private void HandleRotation()
        {
            if (currentView == null || currentView.RotationRoot == null)
                return;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            currentView.RotationRoot.Rotate(Vector3.up, -h * rotationSpeed * Time.deltaTime, Space.World);
            currentView.RotationRoot.Rotate(camera.transform.right, v * rotationSpeed * Time.deltaTime, Space.World);
        }

        private void HandleSocketHover()
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, socketLayer))
            {
                AssemblySocket socket = hit.collider.GetComponentInParent<AssemblySocket>();
                if (socket != currentSocket)
                {
                    ClearSocketHighlight();
                    currentSocket = socket;
                    currentSocket.SetHighlight(true);
                }
            }
            else
            {
                ClearSocketHighlight();
            }
        }

        private void ClearSocketHighlight()
        {
            if (currentSocket != null)
                currentSocket.SetHighlight(false);

            currentSocket = null;
        }

        private void HandleSocketClick()
        {
            if (Input.GetMouseButtonDown(0) && currentSocket != null && !currentSocket.IsFilled)
                OpenInventoryForSocket(currentSocket);
        }

        private void OpenInventoryForSocket(AssemblySocket socket)
        {
            inventoryUI.OpenForAssemblySocket(socket);
        }
    }
}