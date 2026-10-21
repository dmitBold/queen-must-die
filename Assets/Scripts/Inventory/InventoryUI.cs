using Core;
using DI;
using NightCycle;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Zenject;

namespace Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image panel;
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Main List (3 Items)")]
        [SerializeField] private Transform slotsParent;
        [SerializeField] private ItemSlot slotPrefab;
        [SerializeField] private ItemTooltip tooltip;

        [Header("Mini Parts Window")]
        [SerializeField] private GameObject miniPartsPanel;
        [SerializeField] private Transform partsSlotsParent;
        [SerializeField] private ItemSlot partSlotPrefab;

        [SerializeField] private List<ItemData> test_items;

        // State
        public bool isOpen { get; private set; }
        private int currentScrollIndex = 0;
        private List<ItemData> filteredMainItems = new();
        private ItemData selectedItem;
        private List<ItemSlot> activeMainSlots = new();

        private AssemblySocket activeSocket;
        private AssemblyView activeAssemblyView;

        // Dependencies
        private InventoryManager inventory;
        private AssemblyService assemblyService;
        private AudioService audioService;
        private IPlayerProvider playerProvider;

        public CanvasGroup DayUI;

        public bool canOpen = true;

        [Inject]
        public void Construct(InventoryManager inventoryManager, AssemblyService assemblyService, AudioService audioService, IPlayerProvider playerProvider)
        {
            this.inventory = inventoryManager;
            this.assemblyService = assemblyService;
            this.audioService = audioService;

            this.playerProvider = playerProvider;

            this.inventory.OnInventoryChanged += RefreshUI;
        }

        private void Awake()
        {
            // Если поле пустое, ищем объект на сцене по имени
            if (DayUI == null)
            {
                GameObject uiObject = GameObject.Find("UI"); // Убедись, что объект на сцене называется именно "UI"
                if (uiObject != null)
                {
                    DayUI = uiObject.GetComponent<CanvasGroup>();
                }
            }
        }

        private void Start()
        {
            rootCanvas.enabled = false;
            miniPartsPanel.SetActive(false);
        }

        private void Toggle()
        {
            if (isOpen) Close();
            else Open();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                /*if (isOpen) Close();
                else Open();*/
                Toggle();
                /*if (miniPartsPanel.activeSelf)
                {
                    CloseMiniPartsPanel();
                }
                else
                {
                    Toggle();
                }*/
            }

            // ��������� ��������� ���� (������ ���� ������ ��������� � ������� ���� �������)
            if (isOpen && !miniPartsPanel.activeSelf && filteredMainItems.Count > 3)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll > 0f) ScrollList(-1);
                else if (scroll < 0f) ScrollList(1);
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                Debug.Log("TEST ADD ITEM");
                //inventory.AddItem(test);
                foreach (ItemData item in test_items)
                {
                    inventory.AddItem(item);
                }
            }
        }

        private void Open()
        {
            if (!canOpen) { return; }
            isOpen = true;
            playerProvider.PlayerView?.PlayerStateController?.SetMode(PlayerMode.Focused);
            if (DayUI != null)
            {
                DayUI.alpha = 0f;
                DayUI.blocksRaycasts = false;
            }
            rootCanvas.enabled = true;
            panel.gameObject.SetActive(true);
            panel.enabled = true;
            miniPartsPanel.SetActive(false);

            UpdateFilteredItems();

            if (filteredMainItems.Count > 0)
            {
                currentScrollIndex = Mathf.Max(0, filteredMainItems.Count - 3);
                SelectItem(filteredMainItems.Last());
                panel.enabled = false;
            }
            else
            {
                SelectItem(null);
            }

            RefreshUI();
        }

        public void Close()
        {
            playerProvider.PlayerView?.PlayerStateController?.SetMode(PlayerMode.FreeMovement);
            if (DayUI != null)
            {
                DayUI.alpha = 1f;
                DayUI.blocksRaycasts = true;
            }
            isOpen = false;
            rootCanvas.enabled = false;
            miniPartsPanel.SetActive(false);

            if (assemblyService != null && assemblyService.IsActive)
            {
                assemblyService.CloseAssembly();
            }
        }

        public void CloseUIOnly()
        {
            isOpen = false;
            if (rootCanvas != null) rootCanvas.enabled = false;
            if (miniPartsPanel != null) miniPartsPanel.SetActive(false);
            if (panel != null) panel.gameObject.SetActive(false);

            if (DayUI != null)
            {
                DayUI.alpha = 1f;
                DayUI.blocksRaycasts = true;
            }
        }

        public void CloseMiniPartsPanel()
        {
            if (miniPartsPanel != null)
            {
                miniPartsPanel.SetActive(false);
            }

            // ���� �������� ��������� ������, �� ��������� � rootCanvas, ����� �� �� �����
            if (!isOpen && rootCanvas != null)
            {
                rootCanvas.enabled = false;
            }
        }

        private void UpdateFilteredItems()
        {
            filteredMainItems = inventory.GetAllItems()
                .Where(item => item.itemType == ItemType.Normal || item.itemType == ItemType.PortableBase)
                .ToList();
        }

        private void ScrollList(int direction)
        {
            int maxIndex = Mathf.Max(0, filteredMainItems.Count - 3);
            currentScrollIndex = Mathf.Clamp(currentScrollIndex + direction, 0, maxIndex);
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (!isOpen) return;
            UpdateFilteredItems();

            foreach (var slot in activeMainSlots)
            {
                if (slot != null) Destroy(slot.gameObject);
            }
            activeMainSlots.Clear();

            if (currentScrollIndex + 3 > filteredMainItems.Count)
                currentScrollIndex = Mathf.Max(0, filteredMainItems.Count - 3);

            int countToSpawn = Mathf.Min(3, filteredMainItems.Count - currentScrollIndex);
            for (int i = 0; i < countToSpawn; i++)
            {
                ItemData item = filteredMainItems[currentScrollIndex + i];
                ItemSlot slot = Instantiate(slotPrefab, slotsParent);
                slot.Set(item, this, true, tooltip);
                activeMainSlots.Add(slot);
            }

            if (selectedItem != null)
                descriptionText.text = $"<b>{selectedItem.itemName}</b>\n{selectedItem.description}";
            else
                descriptionText.text = "�����";
        }

        public void OnItemClicked(ItemData item, bool isPartClick = false)
        {
            if (isPartClick)
            {
                TryApplyPartToSocket(item);
            }
            else
            {
                SelectItem(item);
            }
        }

        private void SelectItem(ItemData item)
        {
            selectedItem = item;
            if (selectedItem != null && selectedItem.itemType == ItemType.Normal)
            {
                return;
            }
            miniPartsPanel.SetActive(false);

            if (item != null && assemblyService.IsActive) assemblyService.CloseAssembly();

            if (selectedItem != null && selectedItem.itemType == ItemType.PortableBase && selectedItem.assemblyPrefab != null)
            {
                InteractableView viewPrefab = selectedItem.assemblyPrefab.GetComponent<InteractableView>();
                if (viewPrefab != null)
                {
                    assemblyService.OpenAssembly(viewPrefab, OnPortableBaseAssembled, true);
                }
            }

            RefreshUI();
        }

        // --- �������� ������� ---

        public void OpenPartsMenuForSocket(AssemblySocket socket, AssemblyView assemblyView)
        {
            activeSocket = socket;
            activeAssemblyView = assemblyView;

            // ПРОВЕРЯЕМ КОНТЕКСТ: Это PortableBase или объект в мире?
            bool isPortableBase = assemblyService != null && assemblyService.IsPortableBaseSession;

            if (isPortableBase)
            {
                // Для PortableBase ОСТАВЛЯЕМ боковую панель активной
                panel.gameObject.SetActive(true);
                isOpen = true; // Оставляем true, чтобы работал RefreshUI
            }
            else
            {
                //Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA#$#$#$#$#$#$");
                // Для AssemblyInteractable (мира) СКРЫВАЕМ боковую панель
                panel.gameObject.SetActive(false);
                isOpen = false;
            }

            rootCanvas.enabled = true;
            miniPartsPanel.SetActive(true);

            foreach (Transform child in partsSlotsParent)
            {
                Destroy(child.gameObject);
            }

            var availableParts = inventory.GetAllItems().Where(item => item.itemType == ItemType.Part).ToList();

            foreach (var part in availableParts)
            {
                ItemSlot partSlot = Instantiate(partSlotPrefab, partsSlotsParent);
                partSlot.Set(part, this, true, tooltip, isPart: true);
            }
        }

        private void TryApplyPartToSocket(ItemData part)
        {
            if (activeSocket == null) return;

            if (activeSocket.CanAccept(part))
            {
                activeSocket.Apply(part);
                inventory.RemoveItem(part);
                miniPartsPanel.SetActive(false);

                // ���� ��������� �� ��� ������ ���������� (������ ����� ��� ���� "I"), ������ Canvas
                if (!isOpen && rootCanvas != null)
                {
                    rootCanvas.enabled = false;
                }

                RefreshUI();

                if (activeAssemblyView != null)
                {
                    activeAssemblyView.CheckSocketsCompletion();
                }
            }
            else
            {
                Debug.LogWarning("���� ������: �������� ������!");
            }
        }

        private void OnPortableBaseAssembled()
        {
            if (selectedItem == null || selectedItem.resultItem == null) return;

            ItemData completedItem = selectedItem.resultItem;
            inventory.RemoveItem(selectedItem);
            inventory.AddItem(completedItem);

            Debug.Log($"������ �������! �������: {completedItem.itemName}");
            Open();
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.OnInventoryChanged -= RefreshUI;
            }
        }
    }
}