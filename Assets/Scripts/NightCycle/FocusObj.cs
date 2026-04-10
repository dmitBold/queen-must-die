using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static PlayerStateController;
using UnityEngine.Events;

public class FocusObj : MonoBehaviour, IFocusable
{
    [Header("References")]
    [SerializeField] Camera FocusCamera;
    [SerializeField] Camera MainCamera;
    [SerializeField] Canvas FocusCanvas;
    [SerializeField] Transform ModelRoot;
    //[SerializeField] InventoryUI inventoryUI;

    [Header("Rotation")]
    [SerializeField] float rotationSpeed = 80f;

    public CodeLock lockk;

    public bool isActive;

    //test
    //public UnityEvent OnCompleted;
    //[SerializeField] LayerMask socketLayer;

    //AssemblySocket currentSocket;

    //public List<AssemblySocket> sockets;

    public void OnEnterFocus()
    {
        //Debug.Log("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        EnterFocus();
        lockk.Enter();
        //HUDController.instance.EnableInteractionText("E чтобы выйти");
    }

    public void OnExitFocus()
    {
        ExitFocus();
        lockk.Exit();
    }

    public void Test()
    {
        Debug.Log("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }

    void Start()
    {
        FocusCamera.gameObject.SetActive(false);
    }

    public void EnterFocus()
    {
        Debug.Log("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        FocusCanvas.gameObject.SetActive(true);

        isActive = true;

        FocusCamera.gameObject.SetActive(true);
        MainCamera.gameObject.SetActive(false);

        Debug.Log("[Focus] Enter");
    }

    public void ExitFocus()
    {

        FocusCanvas.gameObject.SetActive(false);

        isActive = false;
 
        //PlayerStateController.Instance.SetMode(PlayerMode.FreeMovement);

        if (FocusCamera)
        {
            FocusCamera.gameObject.SetActive(false);
            MainCamera.gameObject.SetActive(true);
        }
        Debug.Log("[Focus] Exit");
    }

    void HandleRotation()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        ModelRoot.Rotate(Vector3.up, -h * rotationSpeed * Time.deltaTime, /*Space.World*/ Space.Self);
        ModelRoot.Rotate(Vector3.right, v * rotationSpeed * Time.deltaTime, /*Space.World*/ Space.Self);
    }

    void Update()
    {
        if (!isActive)
            return;


        HandleRotation();

        /*if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("AAAAA");
        }*/
    }

}
