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
    public class InventoryUI : MonoBehaviour, IUIScreen
    {
        [Tooltip("Идентификатор окна инвентаря. Клавиша открытия задаётся в самом ассете.")]
        [SerializeField] private UIScreenId screenId;

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

        // Пока играет финальная анимация сборки, данные инвентаря уже заменены, а слот показывает старый предмет.
        private ItemData frozenOld;
        private ItemData frozenNew;
        // Основа, с которой стартовала текущая сессия сборки. selectedItem к моменту завершения может уже указывать на другой предмет.
        private ItemData sessionBase;
        private List<ItemSlot> activeMainSlots = new();

        private AssemblySocket activeSocket;
        private AssemblyView activeAssemblyView;

        // Dependencies
        private InventoryManager inventory;
        private AssemblyService assemblyService;
        private AudioService audioService;

        public CanvasGroup DayUI;

        public UIScreenId Id => screenId;

        [InjectOptional] private ItemDatabase itemDatabase;

        [Inject]
        public void Construct(InventoryManager inventoryManager, AssemblyService assemblyService, AudioService audioService, UIManager uiManager)
        {
            this.inventory = inventoryManager;
            this.assemblyService = assemblyService;
            this.audioService = audioService;

            this.inventory.OnInventoryChanged += RefreshUI;
            _uiManager = uiManager;
            uiManager.Register(this);
        }

        private UIManager _uiManager;

        /// <summary>
        /// Оставлен для кнопок и событий в сценах и префабах, назначенных через UnityEvent: делегирует UIManager.
        /// </summary>
        public void Toggle()
        {
            _uiManager.Toggle(screenId);
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

        private void Update()
        {
            // Хоткей I обрабатывает UIInputHandler через UIManager.

            // ��������� ��������� ���� (������ ���� ������ ��������� � ������� ���� �������)
            if (isOpen && !miniPartsPanel.activeSelf && filteredMainItems.Count > 3)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll > 0f) ScrollList(-1);
                else if (scroll < 0f) ScrollList(1);
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                // DEBUG: B - все основы и детали из ItemDatabase (+ test_items), которых ещё нет в инвентаре
                int added = 0;
                var toAdd = new List<ItemData>();
                if (itemDatabase != null)
                    foreach (var item in itemDatabase.AllItems)
                        if (item != null && (item.itemType == ItemType.PortableBase || item.itemType == ItemType.Part)) toAdd.Add(item);
                if (test_items != null) toAdd.AddRange(test_items);
                foreach (ItemData item in toAdd)
                {
                    if (item == null || inventory.HasItem(item)) continue;
                    inventory.AddItem(item);
                    added++;
                }
                Debug.Log($"[DEBUG B] added {added} items (all bases and parts)");
            }
        }

        public void Open()
        {
            // До isOpen = true: RefreshUI внутри ReleaseFreeze не должен перестраивать список раньше времени.
            ReleaseFreeze();
            isOpen = true;
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
            if (DayUI != null)
            {
                DayUI.alpha = 1f;
                DayUI.blocksRaycasts = true;
            }
            isOpen = false;
            rootCanvas.enabled = false;
            miniPartsPanel.SetActive(false);

            // Закрытие окна в обход Esc (BlockAll, загрузка) не должно оставлять сессию сборки висеть.
            if (assemblyService != null && assemblyService.IsActive)
            {
                assemblyService.CloseAssembly();
            }

            // Страховка: если цепочка завершения сборки не дошла до колбэка, слот не должен остаться замороженным.
            ReleaseFreeze();
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
                .Where(IsMainListItem)
                .ToList();

            // Во время заморозки на месте результата показываем старый предмет: слот не пересоздаётся и не мигает.
            if (frozenOld != null)
            {
                int index = filteredMainItems.IndexOf(frozenNew);
                if (index >= 0)
                    filteredMainItems[index] = frozenOld;
            }
        }

        /// <summary>
        /// Part показывается в главной панели наравне с Normal и PortableBase, а в мини-панели остаётся деталью для сокетов.
        /// </summary>
        private static bool IsMainListItem(ItemData item)
        {
            return item.itemType == ItemType.Normal
                || item.itemType == ItemType.PortableBase
                || item.itemType == ItemType.Part;
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

            if (currentScrollIndex + 3 > filteredMainItems.Count)
                currentScrollIndex = Mathf.Max(0, filteredMainItems.Count - 3);

            int countToShow = Mathf.Min(3, filteredMainItems.Count - currentScrollIndex);

            // Слоты переиспользуем: пересоздание заново строит 3D-превью и даёт мигание.
            if (activeMainSlots.Any(s => s == null))
            {
                foreach (var slot in activeMainSlots)
                {
                    if (slot != null) Destroy(slot.gameObject);
                }
                activeMainSlots.Clear();
            }

            while (activeMainSlots.Count > countToShow)
            {
                int last = activeMainSlots.Count - 1;
                // Destroy отложен до конца кадра, поэтому слот сначала скрываем, чтобы он не влиял на раскладку.
                activeMainSlots[last].gameObject.SetActive(false);
                Destroy(activeMainSlots[last].gameObject);
                activeMainSlots.RemoveAt(last);
            }

            for (int i = 0; i < countToShow; i++)
            {
                ItemData item = filteredMainItems[currentScrollIndex + i];
                if (i < activeMainSlots.Count)
                {
                    if (activeMainSlots[i].Item != item)
                        activeMainSlots[i].Set(item, this, true, tooltip);
                }
                else
                {
                    ItemSlot slot = Instantiate(slotPrefab, slotsParent);
                    slot.Set(item, this, true, tooltip);
                    activeMainSlots.Add(slot);
                }
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
                // Замороженный слот показывает уже заменённую основу: сессию по ней открывать нельзя.
                if (frozenOld != null && item == frozenOld)
                    return;

                SelectItem(item);
            }
        }

        private void SelectItem(ItemData item)
        {
            selectedItem = item;
            miniPartsPanel.SetActive(false);

            // Выбор любого предмета завершает активную сессию сборки. Normal и Part на этом заканчиваются:
            // у них нет assemblyPrefab, и больше с ними ничего не происходит.
            if (item != null && assemblyService.IsActive) assemblyService.CloseAssembly();

            if (selectedItem != null && selectedItem.itemType == ItemType.PortableBase && selectedItem.assemblyPrefab != null)
            {
                InteractableView viewPrefab = selectedItem.assemblyPrefab.GetComponent<InteractableView>();
                if (viewPrefab != null)
                {
                    sessionBase = selectedItem;
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
                AssemblySocket socket = activeSocket;

                // Данные меняем до Apply: без анимации Apply синхронно доходит до колбэка завершения и закрывает сессию.
                ItemData result = GetResultIfCompleting(socket);
                bool freeze = result != null && CanFreeze(sessionBase, result);
                if (freeze)
                {
                    frozenOld = sessionBase;
                    frozenNew = result;
                }

                inventory.RemoveItem(part);
                if (result != null)
                {
                    CommitReplacement(sessionBase, result, freeze);
                }
                miniPartsPanel.SetActive(false);

                // ���� ��������� �� ��� ������ ���������� (������ ����� ��� ���� "I"), ������ Canvas
                if (!isOpen && rootCanvas != null)
                {
                    rootCanvas.enabled = false;
                }

                RefreshUI();

                // Завершение сборки проверяет сам AssemblyView по OnFilled сокета, когда заполнение закончено.
                socket.Apply(part);
            }
            else
            {
                Debug.LogWarning("���� ������: �������� ������!");
            }
        }

        /// <summary>
        /// Возвращает результат сборки, если вставка в этот сокет завершит сборку основы из инвентаря. Иначе null.
        /// </summary>
        private ItemData GetResultIfCompleting(AssemblySocket socket)
        {
            if (sessionBase == null || sessionBase.resultItem == null || sessionBase.resultItem == sessionBase)
                return null;

            if (assemblyService == null || !assemblyService.IsPortableBaseSession)
                return null;

            if (activeAssemblyView == null || !activeAssemblyView.IsLastEmptySocket(socket))
                return null;

            return inventory.HasItem(sessionBase) ? sessionBase.resultItem : null;
        }

        // Слот можно держать на месте, только если позиция однозначна: единственная копия основы и новый предмет в списке.
        private bool CanFreeze(ItemData oldItem, ItemData newItem)
        {
            return inventory.GetCount(oldItem) == 1 && !inventory.HasItem(newItem) && IsMainListItem(newItem);
        }

        private void CommitReplacement(ItemData oldItem, ItemData newItem, bool freeze)
        {
            if (!inventory.ReplaceItem(oldItem, newItem))
            {
                frozenOld = null;
                frozenNew = null;
                return;
            }

            // Без заморозки выделение переносим сразу, при заморозке это сделает ReleaseFreeze после анимации.
            if (!freeze && selectedItem == oldItem && !inventory.HasItem(oldItem))
            {
                selectedItem = IsMainListItem(newItem) ? newItem : null;
            }
        }

        /// <summary>
        /// Снимает заморозку слота: слот обновляется на месте, выделение переходит со старого предмета на результат.
        /// </summary>
        private void ReleaseFreeze()
        {
            if (frozenOld == null)
                return;

            ItemData oldItem = frozenOld;
            ItemData newItem = frozenNew;
            frozenOld = null;
            frozenNew = null;

            if (selectedItem == oldItem)
                selectedItem = newItem;

            RefreshUI();
        }

        private void OnPortableBaseAssembled()
        {
            // Инвентарь уже заменён в TryApplyPartToSocket. Здесь только завершается визуальная часть.
            // Пустой resultItem безопасен: подмены не было, замороженного слота тоже нет.
            if (sessionBase == null || sessionBase.resultItem == null) return;

            ItemData completedItem = sessionBase.resultItem;
            ReleaseFreeze();

            Debug.Log($"������ �������! �������: {completedItem.itemName}");
            // Инвентарь остаётся открытым под сборкой, поэтому после неё игрок возвращается в него без повторного Open.
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