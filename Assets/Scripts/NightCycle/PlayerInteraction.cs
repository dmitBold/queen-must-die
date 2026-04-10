using UnityEngine;
using static PlayerStateController;

public class PlayerInteraction : MonoBehaviour
{

    public float PlayerReach = 3f;
    Interactable currentInteractable;
    ItemTarget currentItemTarget;
    bool isInItemSelection;

    [SerializeField] InventoryUI inventoryUI;

    //string prev_msg;
    //test
    private IFocusable activeFocus;
    private float currentHoldTimer = 0f;
    private bool isInteractionProcessed = false;
    //test
    void Update()
    {
        Debug.Log(PlayerStateController.Instance.CurrentMode);


        //test
        if(PlayerStateController.Instance.CurrentMode == PlayerMode.FreeMovement)
        {
            CheckInteraction();
            HandleInteractionInput();
        }
        //test
        //CheckInteraction();


        /*if(Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            //currentInteractable.Interact();
            TryInteract();
        }

        //test
        if(Input.GetKeyDown(KeyCode.E) && inventoryUI.currentMode == InventoryUI.InventoryMode.NightItemSelection && isInItemSelection)
        {
            isInItemSelection = false;
            inventoryUI.ExitItemSelection();
        }*/
        //test
        else if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInteraction();
            //Debug.Log(PlayerStateController.Instance.CurrentMode);
            //Debug.Log(inventoryUI.currentMode);
            //if (PlayerStateController.Instance.CurrentMode == PlayerMode.ItemSelection/*inventoryUI.currentMode == InventoryUI.InventoryMode.NightItemSelection && isInItemSelection*/)
            //{
            //  isInItemSelection = false;
            //currentInteractable.message = prev_msg;
            //inventoryUI.ExitItemSelection();
            //if()
            //inventoryUI.currentMode = InventoryUI.InventoryMode.Day;
            //}
            //else if(PlayerStateController.Instance.CurrentMode == PlayerMode.Assembly/*inventoryUI.currentMode == InventoryUI.InventoryMode.AssemblyItemSelection*/)
            //{
            //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            //  currentInteractable.Interact_Assembly();
            //}
            //else if (currentInteractable != null)
            //{
            //  TryInteract();
            //}
        }

    }

    void ToggleInteraction()
    {
        // Если мы уже с чем-то взаимодействуем - выходим
        if (activeFocus != null)
        {
            activeFocus.OnExitFocus();
            activeFocus = null;
            PlayerStateController.Instance.SetMode(PlayerMode.FreeMovement);
            return;
        }

        // Если мы смотрим на объект и нажали E
        if (currentInteractable != null)
        {
            // Пытаемся взять интерфейс фокуса у объекта
            activeFocus = currentInteractable.GetComponent<IFocusable>();

            if (activeFocus != null)
            {
                activeFocus.OnEnterFocus();
                Debug.Log("AAA!!!");
                PlayerStateController.Instance.SetMode(PlayerMode.Focused);
            }
            else
            {
                // Если это обычный объект (без пазла) - просто вызываем UnityEvent
                currentInteractable.Interact();
            }
        }
    }

    void TryInteract()
    {
        if(currentItemTarget != null)
        {
            EnterItemSelectionMode();
            return;
        }

        if(currentInteractable != null)
        {
            //EnterItemSelectionMode();
            //currentInteractable.Interact();
            ToggleInteraction();
        }

    }

    void EnterItemSelectionMode()
    {
        //Debug.Log("AAAAAAAAAAAAAAAAAAA!!!!!!!!!!!!!!!!!!");
        //prev_msg = currentInteractable.message;
        //currentInteractable.message = "E чтобы выйти";
        HUDController.instance.EnableInteractionText("E чтобы выйти");

        isInItemSelection = true;

        //PlayerStateController.Instance.SetMode(PlayerMode.ItemSelection);

        inventoryUI.OpenForItemTarget(currentItemTarget);
    }

    void CheckInteraction()
    {
        RaycastHit hit;
        if (Camera.main is null)
        {
            Debug.Log("AAA");
        }
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out hit, PlayerReach))
        {
            if (hit.collider.tag == "Interactable")
            {
                Interactable newInteractable = hit.collider.GetComponent<Interactable>();

                //test
                currentItemTarget = hit.collider.GetComponent<ItemTarget>();
                //test

                if (currentInteractable && newInteractable != currentInteractable)
                {
                    currentInteractable.DisableOutline();
                    ResetHoldProgress();
                }

                if (newInteractable.enabled)
                {
                    SetNewCurrentInteractable(newInteractable);
                }
                else
                {
                    DisableCurrentInteractable();
                }

            }
            else
            {
                DisableCurrentInteractable();
            }
        }
        else
        {
            DisableCurrentInteractable();
        }
    }

    void SetNewCurrentInteractable(Interactable newInteractable)
    {
        //if (currentInteractable == newInteractable) return;

        currentInteractable = newInteractable;
        currentInteractable.EnableOutline();
        HUDController.instance.EnableInteractionText(currentInteractable.message);
    }

    /*void DisableCurrentInteractable()
    {
        HUDController.instance.DisableInteractionText();
        if (currentInteractable)
        {
            currentInteractable.DisableOutline();
            //currentInteractable = null;
            if (PlayerStateController.Instance.CurrentMode != PlayerMode.ItemSelection && PlayerStateController.Instance.CurrentMode != PlayerMode.Assembly) // В ItemSelectionMode интерактор фиксирован
            {
                //Debug.Log(inventoryUI.currentMode);
                //currentItemTarget = null;
                currentInteractable = null;
            }
        }
        //TEST
        //if (inventoryUI.currentMode != InventoryUI.InventoryMode.NightItemSelection)
        //{
            //currentItemTarget = null;
        //}

    }*/

    void DisableCurrentInteractable()
    {
        HUDController.instance.DisableInteractionText();

        ResetHoldProgress();

        if (currentInteractable)
        {
            currentInteractable.DisableOutline();

            // Если мы НЕ в режиме фокуса, значит можно забыть про объект
            if (PlayerStateController.Instance.CurrentMode == PlayerMode.FreeMovement)
            {
                currentInteractable = null;
            }
        }
    }

    //TEST TEST TEST
    void HandleInteractionInput()
    {
        if (currentInteractable == null)
        {
            ResetHoldProgress();
            return;
        }

        if (currentInteractable.interactionType == Interactable.InteractionType.Instant)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleInteraction();
            }
        }
        else if (currentInteractable.interactionType == Interactable.InteractionType.Hold)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!isInteractionProcessed)
                {
                    currentHoldTimer += Time.deltaTime;

                    float progress = currentHoldTimer / currentInteractable.holdDuration;
                    HUDController.instance.UpdateProgress(progress);

                    if (currentHoldTimer >= currentInteractable.holdDuration)
                    {
                        ToggleInteraction();
                        isInteractionProcessed = true; 
                        HUDController.instance.HideProgress(); 
                    }
                }
            }
            else
            {
                ResetHoldProgress();
            }
        }
    }

    void ResetHoldProgress()
    {
        currentHoldTimer = 0f;
        isInteractionProcessed = false;
        HUDController.instance.HideProgress();
    }
    //TEST TEST TEST

    //TEST TEST TEST TEST TEST TEST TEST TEST
    public void ForceExitFocus()
    {
        if (activeFocus != null)
        {
            activeFocus.OnExitFocus();
            activeFocus = null;
            PlayerStateController.Instance.SetMode(PlayerMode.FreeMovement);
        }
        else
        {
            Debug.Log("BAD BAD BAD BAD BAD!!!");
            PlayerStateController.Instance.SetMode(PlayerMode.FreeMovement);
        }
    }
    //TEST TEST TEST TEST TEST TEST TEST TEST

}
