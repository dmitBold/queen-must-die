using Inventory;
using NightCycle;
using System;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static Inventory.InventoryUI;

public class QuestUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private QuestSlot slotPrefab;
    [SerializeField] private Canvas rootCanvas;

    [Header("State")] public bool isOpen = false;

    public QuestManager questManager;

    [Inject]
    public void Construct(QuestManager QuestManager)
    {
        questManager = QuestManager;
        questManager.OnQuestPoolChanged += Refresh;
    }

    private void OnDestroy()
    {
        if (questManager != null)
            questManager.OnQuestPoolChanged -= Refresh;
    }

    public void DisableRootCanvas()
    {
        rootCanvas.gameObject.SetActive(false);
    }

    public void EnableRootCanvas()
    {
        rootCanvas.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
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
    }

    private void Refresh()
    {
        if (slotsParent == null) return;

        for (int i = slotsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(slotsParent.GetChild(i).gameObject);
        }

        var quests = questManager.GetQuests();

        foreach (var quest in quests)
        {
            QuestSlot slot = Instantiate(slotPrefab, slotsParent);

            slot.Set(quest, this);
        }
    }
}
