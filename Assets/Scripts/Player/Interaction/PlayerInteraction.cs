using Inventory;
using UnityEngine;
using static NightCycle.PlayerStateController;
using UI;
using Zenject;

namespace NightCycle
{
    public class PlayerInteraction : MonoBehaviour
    {
        #region Constants

        private const string INTERACTABLE_TAG = "Interactable";
        private const string INTERACTION_KEY_TEXT = "E";
        private const float DEFAULT_PLAYER_REACH = 3f;
        private const int MAX_RAY_HITS = 16;

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
        public bool can_interact = true;
        private bool doorHoldCompleted = false;
        private CharacterController characterController;
        private readonly RaycastHit[] rayHits = new RaycastHit[MAX_RAY_HITS];

        #endregion

        #region Dependencies

        private InventoryUI inventoryUI;
        private UIManager uiManager;

        #endregion

        [Inject]
        private void Construct(InventoryUI inventoryUI, UIManager uiManager)
        {
            this.inventoryUI = inventoryUI;
            this.uiManager = uiManager;
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!Input.GetKey(KeyCode.E))
            {
                doorHoldCompleted = false;
            }

            if (playerStateController.CurrentMode == PlayerMode.FreeMovement && can_interact)
            {
                CheckInteraction();
                HandleInteractionInput();
            }
            // Без активного фокуса E в не-Free режиме (окно, сборка, катсцена, драг двери) ничего не делает:
            // иначе устаревший currentInteractable входил бы в фокус сквозь открытое окно.
            else if (activeFocus != null && Input.GetKeyDown(KeyCode.E))
            {
                ToggleInteraction();
            }
            //ДЛЯ ТЕСТОВ - ПОТОМ УБРАТЬ ОТКЛЮЧЕНИЕ ПРИЦЕЛА!!!
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (HUDController.instance.CrosshairImage.isActiveAndEnabled)
                {
                    HUDController.instance.SetCrosshairActivity(false);
                }
                else
                {
                    HUDController.instance.SetCrosshairActivity(true);
                }
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
                uiManager.EndFocus();
                return;
            }

            if (currentInteractable != null)
            {
                activeFocus = currentInteractable.GetComponent<IFocusable>();

                if (activeFocus != null)
                {
                    activeFocus.OnEnterFocus();
                    uiManager.BeginFocus(ForceExitFocus);
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
            //inventoryUI.OpenForItemTarget(currentItemTarget);
        }

        private void CheckInteraction()
        {
            if (Camera.main == null)
            {
                Debug.LogWarning("Main camera not found!");
                return;
            }

            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            if (TryGetInteractionHit(ray, out RaycastHit hit))
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

        private bool TryGetInteractionHit(Ray ray, out RaycastHit nearestHit)
        {
            nearestHit = default;
            bool found = false;
            int hitCount = Physics.RaycastNonAlloc(ray, rayHits, playerReach);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit candidate = rayHits[i];

                // Зоны-триггеры (Volume, чекпоинты) не должны перекрывать то, что находится за ними.
                if (candidate.collider.isTrigger && !candidate.collider.CompareTag(INTERACTABLE_TAG))
                {
                    continue;
                }

                if (!found || candidate.distance < nearestHit.distance)
                {
                    nearestHit = candidate;
                    found = true;
                }
            }

            return found;
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
                    if (currentInteractable.TryGetComponent(out HoldDoor door))
                    {
                        HandleDoorHold(door);
                    }
                    else
                    {
                        HandleHoldInteraction();
                    }
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

        private void HandleDoorHold(HoldDoor door)
        {
            if (doorHoldCompleted || !Input.GetKey(KeyCode.E))
            {
                HUDController.instance.HideProgress();
                return;
            }

            if (door.AdvanceHold(characterController, Time.deltaTime))
            {
                doorHoldCompleted = true;
                HUDController.instance.HideProgress();
                currentInteractable.Interact();
                return;
            }

            HUDController.instance.UpdateProgress(door.Progress);
        }

        private void UpdateHoldProgress()
        {
            if (!isHolding)
            {
                isHolding = true;
                currentInteractable.InteractStartHOLD();
                Debug.Log("№№№№№№№№№№№№№№№№");
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
                currentInteractable.InteractResestHOLD();
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

            uiManager.EndFocus();
        }
    }
}