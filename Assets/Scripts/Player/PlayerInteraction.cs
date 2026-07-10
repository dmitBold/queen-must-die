using Inventory;
using UnityEngine;
using static NightCycle.PlayerStateController;
using Zenject;

namespace NightCycle
{
    public class PlayerInteraction : MonoBehaviour
    {
        #region Constants

        private const string INTERACTABLE_TAG = "Interactable";
        private const string INTERACTION_KEY_TEXT = "E";
        private const float DEFAULT_PLAYER_REACH = 3f;

        #endregion

        #region Serialized Fields

        [SerializeField] private float playerReach = DEFAULT_PLAYER_REACH;
        [SerializeField] private PlayerStateController playerStateController;

        #endregion

        #region Private Fields

        private Interactable currentInteractable;
        private ItemTarget currentItemTarget;
        private bool isInItemSelection;
        private IFocusable activeFocus;
        private float currentHoldTimer = 0f;
        private bool isInteractionProcessed = false;
        private bool isHolding = false;

        #endregion

        #region Dependencies

        private InventoryUI inventoryUI;

        #endregion

        [Inject]
        private void Construct(InventoryUI inventoryUI)
        {
            this.inventoryUI = inventoryUI;
        }

        private void Update()
        {
            if (playerStateController.CurrentMode == PlayerMode.FreeMovement)
            {
                CheckInteraction();
                HandleInteractionInput();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleInteraction();
            }
        }

        private void TryInteract()
        {
            if (currentItemTarget != null)
            {
                EnterItemSelectionMode();
                return;
            }

            if (currentInteractable != null)
            {
                ToggleInteraction();
            }
        }

        private void ToggleInteraction()
        {
            if (activeFocus != null)
            {
                activeFocus.OnExitFocus();
                activeFocus = null;
                playerStateController.SetMode(PlayerMode.FreeMovement);
                return;
            }

            if (currentInteractable != null)
            {
                activeFocus = currentInteractable.GetComponent<IFocusable>();

                if (activeFocus != null)
                {
                    activeFocus.OnEnterFocus();
                    playerStateController.SetMode(PlayerMode.Focused);
                }
                else
                {
                    currentInteractable.Interact();
                }
            }
        }

        private void EnterItemSelectionMode()
        {
            HUDController.instance.EnableInteractionText(INTERACTION_KEY_TEXT);
            isInItemSelection = true;
            inventoryUI.OpenForItemTarget(currentItemTarget);
        }

        private void CheckInteraction()
        {
            if (Camera.main == null)
            {
                Debug.LogWarning("Main camera not found!");
                return;
            }

            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, playerReach))
            {
                if (hit.collider.CompareTag(INTERACTABLE_TAG))
                {
                    ProcessInteractableHit(hit);
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

        private void ProcessInteractableHit(RaycastHit hit)
        {
            Interactable newInteractable = hit.collider.GetComponent<Interactable>();
            currentItemTarget = hit.collider.GetComponent<ItemTarget>();

            if (currentInteractable && newInteractable != currentInteractable)
            {
                currentInteractable.DisableOutline();
                ResetHoldProgress();
            }

            if (newInteractable != null && newInteractable.enabled)
            {
                SetNewCurrentInteractable(newInteractable);
            }
            else
            {
                DisableCurrentInteractable();
            }
        }

        private void SetNewCurrentInteractable(Interactable newInteractable)
        {
            currentInteractable = newInteractable;
            currentInteractable.EnableOutline();

            if (!string.IsNullOrEmpty(currentInteractable.message))
            {
                HUDController.instance.EnableInteractionText(currentInteractable.message);
            }
        }

        private void DisableCurrentInteractable()
        {
            HUDController.instance.DisableInteractionText();
            ResetHoldProgress();

            if (currentInteractable != null)
            {
                currentInteractable.DisableOutline();

                if (playerStateController.CurrentMode == PlayerMode.FreeMovement)
                {
                    currentInteractable = null;
                    currentItemTarget = null;
                }
            }
        }

        private void HandleInteractionInput()
        {
            if (currentInteractable == null)
            {
                ResetHoldProgress();
                return;
            }

            switch (currentInteractable.interactionType)
            {
                case Interactable.InteractionType.Instant:
                    HandleInstantInteraction();
                    break;
                case Interactable.InteractionType.Hold:
                    HandleHoldInteraction();
                    break;
            }
        }

        private void HandleInstantInteraction()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleInteraction();
            }
        }

        private void HandleHoldInteraction()
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!isInteractionProcessed)
                {
                    UpdateHoldProgress();
                }
                currentInteractable.InteractStartHOLD();
            }
            else
            {
                ResetHoldProgress();
            }
        }

        private void UpdateHoldProgress()
        {
            if (!isHolding)
            {
                isHolding = true;
                currentInteractable.InteractStartHOLD();
                Debug.Log("更更更更更更更更");
            }

            currentHoldTimer += Time.deltaTime;
            float progress = currentHoldTimer / currentInteractable.holdDuration;
            HUDController.instance.UpdateProgress(progress);

            if (currentHoldTimer >= currentInteractable.holdDuration)
            {
                isHolding = false;
                ToggleInteraction();
                isInteractionProcessed = true;
                HUDController.instance.HideProgress();
            }
        }

        private void ResetHoldProgress()
        {

            if (isHolding && !isInteractionProcessed)
            {
                Debug.Log("&*&*&*&*&");
            }
            isHolding = false;
            currentHoldTimer = 0f;
            isInteractionProcessed = false;
            HUDController.instance.HideProgress();
        }

        public void ForceExitFocus()
        {
            if (activeFocus != null)
            {
                activeFocus.OnExitFocus();
                activeFocus = null;
            }

            playerStateController.SetMode(PlayerMode.FreeMovement);
        }
    }
}