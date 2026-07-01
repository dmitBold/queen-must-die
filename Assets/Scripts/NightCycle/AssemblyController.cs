using System;
using System.Collections.Generic;
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
        [Header("References")]
        [SerializeField] private CinemachineCamera assemblyCamera;
        [SerializeField] private TextMeshProUGUI _hintText;
        [SerializeField] private Canvas assemblyCanvas;
        [SerializeField] private Transform spawnPoint;

        [Inject] private InventoryUI inventoryUI;

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 80f;

        [Header("Sockets")]
        [SerializeField] private LayerMask socketLayer;

        public UnityEvent OnCompleted;
        public bool isActive;

        private InteractableView currentView;
        private Camera mainCamera;
        private AssemblyView currentAssemblyView;
        private LockView currentLockView;
        private IReadOnlyList<AssemblySocket> activeSockets;
        private AssemblySocket currentSocket;
        private PlayerStateController playerStateController;
        private DiContainer container;
        private InteractableView _subscribedView;

        private readonly Dictionary<InteractableView, InteractableView> _prefabInstances = new();
        private AssemblyService _assemblyService;

        [Inject]
        private void Construct([InjectOptional] PlayerStateController playerStateController, DiContainer container, AssemblyService assemblyService)
        {
            this.playerStateController = playerStateController;
            this.container = container;
            this._assemblyService = assemblyService;
        }

        private void Awake()
        {
            mainCamera = Camera.main;

            if (assemblyCamera)
                assemblyCamera.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!isActive)
                return;

            currentView?.UpdateFocusState();

            if (currentAssemblyView != null)
            {
                HandleSocketHover();
                HandleSocketClick();

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    _assemblyService.CloseAssembly();
                }
            }

            HandleRotation();
        }

        private void OnDestroy()
        {
            if (_subscribedView != null)
                _subscribedView.onInteractionCompleted.RemoveListener(HandleViewInteractionCompleted);
        }

        public void Dispose()
        {
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
                /*if (container == null) Debug.LogError("container is NULL!");
                if (viewPrefab == null) Debug.LogError("viewPrefab is NULL!");
                if (spawnPoint == null) Debug.LogError("spawnPoint is NULL!");
                else if (spawnPoint.transform == null) Debug.LogError("spawnPoint.transform is NULL!");*/

                viewInstance = container.InstantiatePrefabForComponent<InteractableView>(viewPrefab, spawnPoint.transform);
                _prefabInstances[viewPrefab] = viewInstance;
            }

            viewInstance.gameObject.SetActive(true);
            currentView = viewInstance;
            _subscribedView = viewInstance;
            _subscribedView.onInteractionCompleted.AddListener(HandleViewInteractionCompleted);
            SetHintText(currentView.HintText);

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
                activeSockets = null;
            }
        }

        public void EnterAssembly()
        {
            isActive = true;

            assemblyCanvas.gameObject.SetActive(true);
            HUDController.instance.DisableInteractionText();

            currentView?.gameObject.SetActive(true);
            currentView?.OnEnterFocus();

            if (playerStateController != null)
            {
                playerStateController.SetMode(PlayerMode.Focused);
            }

            assemblyCamera.gameObject.SetActive(true);
            assemblyCamera.Priority = 100;
        }

        public void ExitAssembly()
        {
            currentView?.OnExitFocus();
            currentView?.gameObject.SetActive(false);

            assemblyCanvas.gameObject.SetActive(false);
            isActive = false;

            // Вызываем очистку мини-инвентаря, если он был открыт
            if (inventoryUI != null)
            {
                inventoryUI.CloseMiniPartsPanel();
            }

            if (playerStateController != null)
            {
                playerStateController.SetMode(PlayerMode.FreeMovement);
            }

            assemblyCamera.Priority = 0;
            assemblyCamera.gameObject.SetActive(false);
        }

        public void SetHintText(string text)
        {
            if (!string.IsNullOrEmpty(text))
                _hintText.text = text;
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
            currentView.RotationRoot.Rotate(mainCamera.transform.right, v * rotationSpeed * Time.deltaTime, Space.World);
        }

        private void HandleSocketHover()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

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
            {
                if (inventoryUI != null)
                {
                    inventoryUI.OpenPartsMenuForSocket(currentSocket, currentAssemblyView);
                }
            }
        }
    }
}