using TMPro;
using UnityEngine;

public class QuestSlot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] TextMeshProUGUI Description;

    private QuestUI questUI;
    private QuestProgress questProgress;

    public void Set(QuestProgress _questProgress, QuestUI _questUI)
    {
        questUI = _questUI;
        questProgress = _questProgress;

        Name.text = questProgress.Quest.QuestName;

        var currentStage = questProgress.GetCurrentStage();
        Description.text = currentStage != null ? currentStage.StageDescription : " вест выполнен";
    }
}