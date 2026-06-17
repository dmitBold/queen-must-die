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
        public enum InventoryMode
        {
            Default,
            NightItemSelection,
            AssemblyItemSelection,
            Disable
        }

        public bool set_active_on_start = false;

        [Header("UI References")] [SerializeField]
        private GameObject panel;

        [SerializeField] private Transform slotsParent;
        [SerializeField] private ItemSlot slotPrefab;
        [SerializeField] private ItemTooltip tooltip;
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private Image dragIconPrefab;

        [Header("State")] public bool isOpen;
        public CardManager cardManager;
        public AssemblySocket currentAssemblySocket;

        private InventoryManager inventory;
        private DayCycleController dayCycle;

        private ItemData draggedItem;
        private DragIconController dragIcon;
        private ItemTarget currentTarget;
        private InventoryMode currentMode;

        public event Action OnSocketFilled;

        [Inject]
        public void Construct(InventoryManager inventoryManager)
        {
            inventory = inventoryManager;
            inventory.OnInventoryChanged += Refresh;
        }

        private void OnDestroy()
        {
            if (inventory != null)
                inventory.OnInventoryChanged -= Refresh;
        }

        private void Start()
        {
            SetMode(InventoryMode.Disable);
            if (set_active_on_start)
            {
                SetMode(InventoryMode.Default);
            }
        }

        public void DisableRootCanvas()
        {
            rootCanvas.gameObject.SetActive(false);
        }

        public void EnableRootCanvas()
        {
            rootCanvas.gameObject.SetActive(true);
        }

        public void Init(DayCycleController dayCycle)
        {
            SetMode(InventoryMode.Default);
            this.dayCycle = dayCycle;
            Refresh();
            Close();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            if (!CanOpen())
                return;

            if (isOpen)
                Close();
            else
                Open();
        }

        private bool CanOpen()
        {
            return true;
        }

        private void Open()
        {
            isOpen = true;
            panel.SetActive(true);
            Refresh();
        }

        private void Close()
        {
            isOpen = false;
            panel.SetActive(false);
            tooltip.Hide();
            EndDrag();
        }

        private void Refresh()
        {
            foreach (Transform child in slotsParent)
            {
                Destroy(child.gameObject);
            }

            var items = inventory.GetAllItems();

            foreach (var item in items)
            {
                ItemSlot slot = Instantiate(slotPrefab, slotsParent);

                bool canApply = false;
                if (currentMode == InventoryMode.Default)
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

                slot.Set(item, this, canApply, tooltip);
            }
        }

        public void BeginDrag(ItemData item, Sprite icon)
        {
            draggedItem = item;

            var obj = Instantiate(dragIconPrefab, rootCanvas.transform);
            dragIcon = obj.GetComponent<DragIconController>();

            dragIcon.SetItem(icon);
            dragIcon.SetResult(null);
        }

        public void UpdateDrag(Vector2 screenPos)
        {
            if (dragIcon)
                dragIcon.transform.position = screenPos;
        }

        public void EndDrag()
        {
            if (dragIcon)
                Destroy(dragIcon.gameObject);

            dragIcon = null;
            draggedItem = null;
        }

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

        public void UpdateDropIndicator(bool isOverDropZone)
        {
            if (dragIcon == null)
                return;

            if (!isOverDropZone || draggedItem == null || !cardManager.wait_for_choice)
            {
                dragIcon.SetResult(null);
                return;
            }

            bool canApply = cardManager.CanApplyItem(draggedItem);
            dragIcon.SetResult(canApply);
        }

        public void OpenForItemTarget(ItemTarget target)
        {
            currentMode = InventoryMode.NightItemSelection;
            currentTarget = target;
            Open();
        }

        public void OpenForAssemblySocket(AssemblySocket socket)
        {
            currentMode = InventoryMode.AssemblyItemSelection;
            currentAssemblySocket = socket;
            Open();
        }

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
                OnSocketFilled?.Invoke();
                inventory.RemoveItem(item);
                Refresh();
                return;
            }
        }
        // тут был баг с тем что когда выходим из замка то блять пропадает inventoryRoot
        public void ExitItemSelection()
        {
            //currentMode = InventoryMode.Default;
            SetMode(InventoryMode.Default);
            currentTarget = null;
            Close();
        }

        public void ExitSelection()
        {
            //currentMode = InventoryMode.Default;
            SetMode(InventoryMode.Default);
            currentAssemblySocket = null;
            Close();
        }

        public bool IsDefaultMode()
        {
            return currentMode == InventoryMode.Default;
        }

        public void SetMode(InventoryMode mode)
        {
            //Debug.Log(mode);
            currentMode = mode;
            rootCanvas.gameObject.SetActive(mode != InventoryMode.Disable);
        }
    }
}