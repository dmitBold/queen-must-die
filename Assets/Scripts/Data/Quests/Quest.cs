using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestStage
{
    [TextArea(3, 5)]
    public string StageDescription;
}

[CreateAssetMenu(fileName = "Quest", menuName = "Scriptable Objects/Quest")]
public class Quest : ScriptableObject
{
    [Header("Base Info")]
    public string id;

    [TextArea(3, 5)]
    public string QuestName;

    public List<QuestStage> QuestStages;
}