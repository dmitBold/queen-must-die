using Core;
using Inventory;
using NightCycle;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class QuestUI : MonoBehaviour, IUIScreen
{
    [Tooltip("Идентификатор окна журнала квестов. Клавиша открытия задаётся в самом ассете.")]
    [SerializeField] private UIScreenId screenId;

    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [Tooltip("Контейнер слотов секции «Основные» (только слоты, без заголовка секции).")]
    [SerializeField] private Transform mainSlotsParent;
    [Tooltip("Контейнер слотов секции «Побочные» (только слоты, без заголовка секции).")]
    [SerializeField] private Transform sideSlotsParent;
    [SerializeField] private QuestSlot slotPrefab;
    [Tooltip("Правая панель: арт и описание выбранного квеста.")]
    [SerializeField] private QuestDetailsView detailsView;
    [SerializeField] private Canvas rootCanvas;


    private Coroutine typingCoroutine;

    [Header("State")] public bool isOpen = false;

    public QuestManager questManager;

    private UIManager _uiManager;

    // Порядок секции: сначала незавершённые квесты, затем завершённые.
    private static readonly bool[] CompletionOrder = { false, true };

    // Слоты переиспользуются между обновлениями, чтобы не терять раскрытие и не пересоздавать список.
    private readonly Dictionary<Quest, QuestSlot> _slots = new();
    private readonly List<Quest> _staleQuests = new();
    private Quest _selectedQuest;

    [Inject]
    public void Construct(QuestManager QuestManager, UIManager uiManager)
    {
        questManager = QuestManager;
        questManager.OnQuestPoolChanged += Refresh;
        _uiManager = uiManager;
        uiManager.Register(this);
    }

    public UIScreenId Id => screenId;

    /// <summary>
    /// Оставлен для кнопок и событий в сценах и префабах, назначенных через UnityEvent: делегирует UIManager.
    /// </summary>
    public void Toggle()
    {
        _uiManager.Toggle(screenId);
    }

    private void OnDestroy()
    {
        if (questManager != null)
        {
            questManager.OnQuestPoolChanged -= Refresh;
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

    public void Open()
    {
        isOpen = true;
        panel.SetActive(true);
        ClearSelection();
        Refresh();
    }

    public void Close()
    {
        isOpen = false;
        panel.SetActive(false);
    }

    private void Refresh()
    {
        if (mainSlotsParent == null || sideSlotsParent == null) return;

        var activeQuests = questManager.GetActiveQuests();

        RemoveStaleSlots(activeQuests);

        foreach (var questProgress in activeQuests)
        {
            if (!_slots.TryGetValue(questProgress.Quest, out QuestSlot slot))
            {
                slot = CreateSlot(questProgress.Quest);
            }

            slot.Set(questProgress);
        }

        ArrangeSection(activeQuests, true);
        ArrangeSection(activeQuests, false);

        if (_selectedQuest != null && !_slots.ContainsKey(_selectedQuest))
        {
            ClearSelection();
        }

        UpdateSlotHighlight();
    }

    private QuestSlot CreateSlot(Quest quest)
    {
        QuestSlot slot = Instantiate(slotPrefab, quest.IsMain ? mainSlotsParent : sideSlotsParent);
        slot.Selected += HandleSlotSelected;
        _slots.Add(quest, slot);
        return slot;
    }

    private void RemoveStaleSlots(List<QuestProgress> activeQuests)
    {
        _staleQuests.Clear();
        foreach (var quest in _slots.Keys)
        {
            if (!activeQuests.Exists(progress => progress.Quest == quest))
            {
                _staleQuests.Add(quest);
            }
        }

        foreach (var quest in _staleQuests)
        {
            Destroy(_slots[quest].gameObject);
            _slots.Remove(quest);
        }
    }

    // Порядок слотов внутри секции: незавершённые, затем завершённые; внутри группы порядок добавления квестов.
    private void ArrangeSection(List<QuestProgress> activeQuests, bool isMain)
    {
        int siblingIndex = 0;
        foreach (bool isCompleted in CompletionOrder)
        {
            foreach (var questProgress in activeQuests)
            {
                if (questProgress.Quest.IsMain == isMain && questProgress.IsCompleted == isCompleted)
                {
                    _slots[questProgress.Quest].transform.SetSiblingIndex(siblingIndex++);
                }
            }
        }
    }

    private void HandleSlotSelected(QuestSlot slot)
    {
        _selectedQuest = slot.Quest;
        detailsView.Show(_selectedQuest);
        UpdateSlotHighlight();
    }

    private void ClearSelection()
    {
        _selectedQuest = null;
        detailsView.Clear();
        UpdateSlotHighlight();
    }

    private void UpdateSlotHighlight()
    {
        foreach (var pair in _slots)
        {
            pair.Value.SetSelected(pair.Key == _selectedQuest);
        }
    }

}
