using System;
using Cards;
using Core;
using NightCycle;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        //[SerializeField] List<ItemSlot> slots;
        [SerializeField] Transform slotsParent;
        [SerializeField] ItemSlot slotPrefab;
        [SerializeField] ItemTooltip tooltip;

        InventoryManager inventory;
        DayCycleController dayCycle;

        [Inject]
        public void Constructor(InventoryManager inventoryManager)
        {
            inventory = inventoryManager;
        }

        public bool isOpen;

        //
        [SerializeField] Canvas rootCanvas;
        [SerializeField] Image dragIconPrefab;

        //Image dragIcon;
        ItemData draggedItem;

        //test
        DragIconController dragIcon;
        //[SerializeField] Image canApplyIcon;
        //[SerializeField] Image cannotApplyIcon;

        //test
        public CardManager cardManager;

        //test
        public enum InventoryMode
        {
            Day,
            NightItemSelection,
            AssemblyItemSelection
        }

        public InventoryMode currentMode;
        ItemTarget currentTarget;


        //test
        public AssemblySocket currentAssemblySocket;
        public event Action OnSocketFilled;
        //test

        public void Init(DayCycleController dayCycle)
        {
            this.dayCycle = dayCycle;

            //ItemSlot.SetTooltip(tooltip);

            Refresh();
            Close();
        }

        void Update()
        {
            Debug.Log("%%%%%" + currentMode + "%%%%%");
            if(currentAssemblySocket != null)
                Debug.Log("######" + currentAssemblySocket.name + "######");
            if (Input.GetKeyDown(KeyCode.I))
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            if (!CanOpen())
                return;

            if (isOpen) Close();
            else Open();
        }

        bool CanOpen()
        {
            //return dayCycle.IsWaitingForChoice();
            //test
            return true;
        }

        void Open()
        {
            isOpen = true;
            panel.SetActive(true);
            Refresh();
        }

        void Close()
        {
            isOpen = false;
            panel.SetActive(false);
            tooltip.Hide();
            EndDrag();
        }

        void Refresh()
        {
            foreach (Transform child in slotsParent)
            {
                Destroy(child.gameObject);
            }

            var items = inventory.GetAllItems();

            foreach (var item in items)
            {
                ItemSlot slot = Instantiate(slotPrefab, slotsParent);

                //test
                //bool canApply = currentMode == InventoryMode.Day
                //      || currentTarget.CanApply(item) || (currentMode == InventoryMode.AssemblyItemSelection && currentAssemblySocket.CanAccept(item));

                bool canApply = false;
                if (currentMode == InventoryMode.Day)
                {
                    canApply = true;
                }
                else if (currentMode == InventoryMode.NightItemSelection)
                {
                    canApply = currentTarget != null && currentTarget.CanApply(item);
                }
                else if (currentMode == InventoryMode.AssemblyItemSelection)
                {
                    canApply = currentAssemblySocket != null && currentAssemblySocket.CanAccept(item);
                }
                //test

                slot.Set(item, this, canApply, tooltip);
            }
        }

        //
        /*public void BeginDrag(ItemData item, Sprite icon)
    {
        //test
        //canApplyIcon.gameObject.SetActive(false);
        //cannotApplyIcon.gameObject.SetActive(false);
        //Debug.Log("BeginDrag");
        draggedItem = item;
        dragIcon = Instantiate(dragIconPrefab, rootCanvas.transform);
        dragIcon.sprite = icon;

        var cg = dragIcon.GetComponent<CanvasGroup>();
        if (cg) cg.alpha = 0.6f;

        dragIcon.raycastTarget = false;
    }*/

        public void BeginDrag(ItemData item, Sprite icon)
        {
            Debug.Log("BeginDrag");
            draggedItem = item;

            var obj = Instantiate(dragIconPrefab, rootCanvas.transform);
            dragIcon = obj.GetComponent<DragIconController>();

            dragIcon.SetItem(icon);
            dragIcon.SetResult(null);
        }

        public void UpdateDrag(Vector2 screenPos)
        {
            Debug.Log("UpdateDrag");
            if (dragIcon)
                dragIcon.transform.position = screenPos;
        }

        /*public void EndDrag()
    {
        //Debug.Log("EndDrag");
        if (dragIcon)
            Destroy(dragIcon.gameObject);

        canApplyIcon.gameObject.SetActive(false);
        cannotApplyIcon.gameObject.SetActive(false);

        draggedItem = null;
    }*/

        public void EndDrag()
        {
            Debug.Log("EndDrag");
            if (dragIcon)
                Destroy(dragIcon.gameObject);

            dragIcon = null;
            draggedItem = null;
        }

        //
        public ItemData GetDraggedItem()
        {
            return draggedItem;
        }

        public void ConsumeDraggedItem()
        {
            inventory.RemoveItem(draggedItem);
            EndDrag();
            Refresh();
        }

        public void CancelDrag()
        {
            EndDrag();
        }

        //test
        /*public void UpdateDropIndicator(bool isOverDropZone)
    {
        if (draggedItem == null || !isOverDropZone)
        {
            canApplyIcon.gameObject.SetActive(false);
            cannotApplyIcon.gameObject.SetActive(false);
            return;
        }

        bool canApply = cardManager.CanApplyItem(draggedItem);

        canApplyIcon.gameObject.SetActive(canApply);
        cannotApplyIcon.gameObject.SetActive(!canApply);
    }*/

        public void UpdateDropIndicator(bool isOverDropZone)
        {
            if (dragIcon == null)
                return;
            //test
            if (!isOverDropZone || draggedItem == null || !cardManager.wait_for_choice)
            {
                dragIcon.SetResult(null);
                return;
            }

            bool canApply = cardManager.CanApplyItem(draggedItem);
            dragIcon.SetResult(canApply);
        }

        //test

        public void OpenForItemTarget(ItemTarget target)
        {
            currentMode = InventoryMode.NightItemSelection;
            currentTarget = target;

            Open();
            //PlayerStateController.Instance.SetMode(PlayerMode.ItemSelection);
        }

        /*void Start()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance is NULL");
            return;
        }
        else 
        {
            inventory = InventoryManager.Instance;
            Debug.Log("OKOKOK");
            Debug.Log("InventoryUI instance: " + GetInstanceID());
            Debug.Log("ITEMS COUNT: " + inventory.Items.Count);
        }
            
        //inventory.OnInventoryChanged += Refresh;
    }*/


        void OnEnable()
        {
                inventory.OnInventoryChanged += Refresh;
        }

        private void OnDisable()
        {
            if (inventory != null)
                inventory.OnInventoryChanged -= Refresh;
        }


        //test
        /*public void OnItemClicked(ItemData item)
    {
        if (currentMode != InventoryMode.NightItemSelection)
            return;

        if (!currentTarget.CanApply(item))
            return;

        currentTarget.Apply(item);
        inventory.RemoveItem(item);

        //ExitItemSelection();
        //test
        Refresh();
        //test
    }*/
        //test
        public void OnItemClicked(ItemData item)
        {
            if (currentMode == InventoryMode.NightItemSelection)
            {
                if (!currentTarget.CanApply(item))
                    return;

                currentTarget.Apply(item);
                inventory.RemoveItem(item);
                Refresh();
                return;
            }

            if (currentMode == InventoryMode.AssemblyItemSelection)
            {
                if (!currentAssemblySocket.CanAccept(item))
                    return;

                currentAssemblySocket.Apply(item);
                //test
                OnSocketFilled.Invoke();
                inventory.RemoveItem(item);
                Refresh();
                return;
            }
        }
        //test
        public void ExitItemSelection()
        {
            //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
            currentMode = InventoryMode.Day;
            currentTarget = null;

            Close();
            //PlayerStateController.Instance.SetMode(PlayerMode.FreeMovement);
        }

        public void ExitAssemblySelection()
        {
            //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
            currentMode = InventoryMode.Day;
            currentAssemblySocket = null;

            Close();
            //PlayerStateController.Instance.SetMode(PlayerMode.FreeMovement);
        }

        public bool IsDayMode()
        {
            return currentMode == InventoryMode.Day;
        }

        //test

        //test
        public void OpenForAssemblySocket(AssemblySocket socket)
        {
            currentMode = InventoryMode.AssemblyItemSelection;
            currentAssemblySocket = socket;

            Open();
            //PlayerStateController.Instance.SetMode(PlayerMode.ItemSelection);
        }
        //test

    }
}
