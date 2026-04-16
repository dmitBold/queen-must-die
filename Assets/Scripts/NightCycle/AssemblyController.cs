using System.Collections.Generic;
using Inventory;
using UnityEngine;
using UnityEngine.Events;
using static NightCycle.PlayerStateController;

namespace NightCycle
{
    public class AssemblyController : MonoBehaviour, IFocusable
    {
        [Header("References")]
        [SerializeField] Camera assemblyCamera;
        //TEST!!!!!!!!!!!!!!!!!!!!!!!!
        [SerializeField] Camera MainCamera;
        [SerializeField] Canvas AssemblyCanvas;
        //TEST!!!!!!!!!!!!!!!!!!!!!!!!
        [SerializeField] Transform assemblyRoot; // ������
        [SerializeField] InventoryUI inventoryUI;

        [Header("Rotation")]
        [SerializeField] float rotationSpeed = 80f;

        //test
        public UnityEvent OnCompleted;
        [SerializeField] LayerMask socketLayer;

        AssemblySocket currentSocket;

        public List<AssemblySocket> sockets;
        //test

        public bool isActive;

        //test
        public void OnEnterFocus()
        {
            EnterAssembly(); 
            HUDController.instance.EnableInteractionText("E ����� �����");
        }

        public void OnExitFocus()
        {
            ExitAssembly();
            //test
            Outline outline = GetComponent<Outline>();
            outline.Rebuild();
            outline.enabled = false;
            outline.enabled = true;
            //test
        }
        //test

        void Awake()
        {
            if (assemblyCamera)
                assemblyCamera.gameObject.SetActive(false);

            inventoryUI.OnSocketFilled += OnAssemblyCompleted;
        }

        void Update()
        {
            if (!isActive)
                return;


            //test
            HandleSocketHover();
            HandleSocketClick();
            HandleRotation();

            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("AAAAA");
                //ExitAssembly();
            }
        }

        //test
        bool checkSockets()
        {
            foreach(var socket in sockets)
            {
                Debug.Log(socket.name);
                if (!socket.IsFilled)
                {
                    return false;
                }
            }
            return true;
        }

        void OnAssemblyCompleted()
        {
            if (!checkSockets())
                return;
            Debug.Log("ASSEMBLY_COMPLETED");
            //TEST
            OnCompleted?.Invoke();
            inventoryUI.OnSocketFilled -= OnAssemblyCompleted;
            //TEST
        }
        //test


        public void EnterAssembly()
        {
            AssemblyCanvas.gameObject.SetActive(true);

            isActive = true;
            //PlayerStateController.Instance.SetMode(PlayerMode.Assembly);
            //test
            inventoryUI.currentMode = InventoryUI.InventoryMode.AssemblyItemSelection;
            //test

            if (assemblyCamera)
            {
                assemblyCamera.gameObject.SetActive(true);
                MainCamera.gameObject.SetActive(false);
            }

            Debug.Log("[Assembly] Enter");
        }

        public void ExitAssembly()
        {

            AssemblyCanvas.gameObject.SetActive(false);

            isActive = false;
            //test
            inventoryUI.ExitAssemblySelection();
            //inventoryUI.currentAssemblySocket = null;
            //inventoryUI.currentMode = InventoryUI.InventoryMode.Day;
            //test
            PlayerStateController.Instance.SetMode(PlayerMode.FreeMovement);

            if (assemblyCamera)
            {
                assemblyCamera.gameObject.SetActive(false);
                MainCamera.gameObject.SetActive(true);
            }
            Debug.Log("[Assembly] Exit");
        }

        void HandleRotation()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            assemblyRoot.Rotate(Vector3.up, -h * rotationSpeed * Time.deltaTime, /*Space.World*/ Space.Self);
            assemblyRoot.Rotate(Vector3.right, v * rotationSpeed * Time.deltaTime, /*Space.World*/ Space.Self);
        }

        //test
        void HandleSocketHover()
        {
            Ray ray = assemblyCamera.ScreenPointToRay(Input.mousePosition);

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

        void ClearSocketHighlight()
        {
            if (currentSocket != null)
                currentSocket.SetHighlight(false);

            currentSocket = null;
        }

        void HandleSocketClick()
        {
            if (Input.GetMouseButtonDown(0) && currentSocket != null)
            {
                if (currentSocket.IsFilled)
                    return;

                OpenInventoryForSocket(currentSocket);
            }
        }

        void OpenInventoryForSocket(AssemblySocket socket)
        {
            Debug.Log("Selected socket: " + socket.name);
            inventoryUI.OpenForAssemblySocket(socket);
        }

        //test
    }
}
