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
    [SerializeField] private Transform slotsParent;
    [SerializeField] private QuestSlot slotPrefab;
    [SerializeField] private Canvas rootCanvas;


    private Coroutine typingCoroutine;

    [Header("State")] public bool isOpen = false;

    public QuestManager questManager;

    private UIManager _uiManager;

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
        Refresh();
    }

    public void Close()
    {
        isOpen = false;
        panel.SetActive(false);
    }

    private void Refresh()
    {
        if (slotsParent == null) return;

        for (int i = slotsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(slotsParent.GetChild(i).gameObject);
        }

        var activeQuests = questManager.GetActiveQuests();

        foreach (var questProgress in activeQuests)
        {
            QuestSlot slot = Instantiate(slotPrefab, slotsParent);
            slot.Set(questProgress, this);
        }
    }

}
