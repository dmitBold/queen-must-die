using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quests_SO/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    [SerializeField] private List<Quest> allQuests;

    private Dictionary<string, Quest> _questDict;

    private void Initialize()
    {
        _questDict = new Dictionary<string, Quest>();
        foreach (var quest in allQuests)
        {
            if (quest != null && !string.IsNullOrEmpty(quest.id))
            {
                _questDict[quest.id] = quest;
            }
        }
    }

    public Quest GetQuestById(string id)
    {
        if (_questDict == null || _questDict.Count == 0)
        {
            Initialize();
        }

        if (_questDict.TryGetValue(id, out var quest))
        {
            return quest;
        }

        Debug.LogError($"[QuestDatabase]  вест с ID {id} не найден!");
        return null;
    }
}