using Inventory;
using System.Collections;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuestSlot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] TextMeshProUGUI Description;

    private QuestUI questUI;

    Quest quest;

    public void Set(Quest _quest, QuestUI _questUI)
    {
        questUI = _questUI;
        quest = _quest;

        Name.text = _quest.name;
        Description.text = quest.currentStage.StageDescription;
    }

}
