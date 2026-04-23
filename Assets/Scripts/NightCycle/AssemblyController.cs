using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Inventory;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using Unity.Cinemachine;

namespace NightCycle
{
    public class AssemblyController : MonoBehaviour, IFocusable, IDisposable
    {
        [Header("References")] [SerializeField]
        private CinemachineCamera assemblyCamera;

        [SerializeField] private Camera camera;
        [SerializeField] private Canvas assemblyCanvas;
        [SerializeField] private Transform spawnPoint;
        [Inject] private InventoryUI inventoryUI;

        [Header("Rotation")] [SerializeField] private float rotationSpeed = 80f;

        [Header("Sockets")] [SerializeField] private LayerMask socketLayer;

        public UnityEvent OnCompleted;
        public bool isActive;

        private AssemblyView currentView;
        private IReadOnlyList<AssemblySocket> activeSockets;
        private AssemblySocket currentSocket;
        private PlayerStateController playerStateController;

        [Inject]
        private void Construct(PlayerStateController playerStateController)
        {
            this.playerStateController = playerStateController;
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

        private void OnDestroy()
        {
            inventoryUI.OnSocketFilled -= OnAssemblyCompleted;
        }

        public void OnEnterFocus()
        {
            EnterAssembly();
            HUDController.instance.EnableInteractionText("E выйти назад");
        }

        public void OnExitFocus()
        {
            ExitAssembly();

            var outline = GetComponent<Outline>();
            outline.Rebuild();
            outline.enabled = false;
            outline.enabled = true;
        }
        
        public void Dispose()
        {
            inventoryUI.OnSocketFilled -= OnAssemblyCompleted;
            activeSockets = null;
            Destroy(currentView);
        }

        /// <summary>
        /// Принимает вью-префаб, спавнит его и запускает режим сборки.
        /// </summary>
        public void InitializeAssembly(AssemblyView viewPrefab)
        {
            currentView = Instantiate(viewPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
        }

        public void EnterAssembly()
        {
            isActive = true;

            assemblyCanvas.gameObject.SetActive(true);
            HUDController.instance.DisableInteractionText();

            inventoryUI.SetMode(InventoryUI.InventoryMode.AssemblyItemSelection);
            playerStateController.SetMode(PlayerMode.Focused);

            assemblyCamera.gameObject.SetActive(true);
            assemblyCamera.Priority = 100;
        }

        public void ExitAssembly()
        {
            assemblyCanvas.gameObject.SetActive(false);
            isActive = false;
            inventoryUI.ExitAssemblySelection();
            playerStateController.SetMode(PlayerMode.FreeMovement);

            assemblyCamera.Priority = 0;
            assemblyCamera.gameObject.SetActive(false);
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