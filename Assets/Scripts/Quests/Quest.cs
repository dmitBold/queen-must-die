using Cards;
using Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class QuestStage
{
    [TextArea(3, 5)]
    public string StageDescription;

    public UnityEvent OnStageFinished;
}

[CreateAssetMenu(fileName = "Quest", menuName = "Scriptable Objects/Quest")]
public class Quest : ScriptableObject
{
    public List<QuestStage> QuestStages;

    [TextArea(3, 5)]
    public string QuestName;

    public UnityEvent OnQuestFinished;
    public bool isCompleted = false;

    public int currentStageindex = 0;
    public QuestStage currentStage;

    private void OnEnable()
    {
        ResetQuest();
    }

    public void ResetQuest()
    {
        currentStageindex = 0;
        isCompleted = false;

        if (QuestStages != null && QuestStages.Count > 0)
        {
            currentStage = QuestStages[0];
        }
        else
        {
            currentStage = null;
        }
    }

    public void AdvanceStage()
    {
        if (currentStageindex < QuestStages.Count - 1)
        {
            currentStageindex++;
            currentStage = QuestStages[currentStageindex];
        }
    }
}
