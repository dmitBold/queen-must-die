using System.Collections.Generic;
using Inventory;
using UnityEngine;
using UnityEngine.Events;
using static NightCycle.PlayerStateController;
using Zenject;
using Unity.Cinemachine;

namespace NightCycle
{
    /// <summary>
    /// Управляет режимом сборки: принимает вью-объект извне, спавнит его,
    /// переключает камеру, обрабатывает ввод для вращения и выбора сокетов.
    /// </summary>
    public class AssemblyController : MonoBehaviour, IFocusable
    {
        [Header("References")]
        [SerializeField] private CinemachineCamera assemblyCamera;
        [SerializeField] private Camera  camera;
        [SerializeField] private Canvas assemblyCanvas;
        [SerializeField] private Transform spawnPoint;
        [Inject] private InventoryUI inventoryUI;

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 80f;

        [Header("Sockets")]
        [SerializeField] private LayerMask socketLayer;

        public UnityEvent OnCompleted;
        public bool isActive;

        private AssemblyView currentView;
        private GameObject spawnedInstance;
        private IReadOnlyList<AssemblySocket> activeSockets;
        private AssemblySocket currentSocket;


        public void OnEnterFocus()
        {
            EnterAssembly();
            HUDController.instance.EnableInteractionText("E выйти назад");
        }

        public void OnExitFocus()
        {
            ExitAssembly();

            Outline outline = GetComponent<Outline>();
            outline.Rebuild();
            outline.enabled = false;
            outline.enabled = true;
        }

        private void Awake()
        {
            if (assemblyCamera)
                assemblyCamera.gameObject.SetActive(false);

            inventoryUI.OnSocketFilled += OnAssemblyCompleted;
        }

        private void Update()
        {
            if (!isActive)
                return;

            HandleSocketHover();
            HandleSocketClick();
            HandleRotation();
        }

        /// <summary>
        /// Принимает вью-префаб, спавнит его и запускает режим сборки.
        /// </summary>
        public void StartAssembly(AssemblyView viewPrefab)
        {
            if (spawnedInstance != null)
                Destroy(spawnedInstance);

            spawnedInstance = Instantiate(viewPrefab.gameObject, spawnPoint.position, spawnPoint.rotation, spawnPoint);
            currentView = spawnedInstance.GetComponent<AssemblyView>();

            if (currentView == null)
            {
                Debug.LogError("[AssemblyController] Spawned prefab does not have AssemblyView component.");
                return;
            }

            activeSockets = currentView.Sockets;
            EnterAssembly();
        }

        public void EnterAssembly()
        {
            assemblyCanvas.gameObject.SetActive(true);
            isActive = true;
            inventoryUI.currentMode = InventoryUI.InventoryMode.AssemblyItemSelection;

            PlayerStateController.Instance.SetMode(PlayerMode.Focused);
            
            if (HUDController.instance != null)
                HUDController.instance.DisableInteractionText();

            if (assemblyCamera)
            {
                assemblyCamera.gameObject.SetActive(true);
                assemblyCamera.Priority = 100;
            }
        }

        public void ExitAssembly()
        {
            assemblyCanvas.gameObject.SetActive(false);
            isActive = false;
            inventoryUI.ExitAssemblySelection();
            PlayerStateController.Instance.SetMode(PlayerMode.FreeMovement);

            if (assemblyCamera)
            {
                assemblyCamera.Priority = 0;
                assemblyCamera.gameObject.SetActive(false);
            }

            if (spawnedInstance != null)
            {
                Destroy(spawnedInstance);
                spawnedInstance = null;
                currentView = null;
                activeSockets = null;
            }
        }

        private bool CheckSockets()
        {
            if (activeSockets == null)
                return false;

            foreach (var socket in activeSockets)
            {
                if (!socket.IsFilled)
                    return false;
            }
            return true;
        }

        private void OnAssemblyCompleted()
        {
            if (!CheckSockets())
                return;

            if (currentView != null)
                currentView.onAssemblyCompleted?.Invoke();

            OnCompleted?.Invoke();
            inventoryUI.OnSocketFilled -= OnAssemblyCompleted;
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
