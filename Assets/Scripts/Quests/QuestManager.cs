using Inventory;
using System;
using System.Collections.Generic;
using Zenject;
using static SaveManager;

public class QuestManager /*: MonoBehaviour*/
{
    public event Action OnQuestPoolChanged;

    private List<Quest> Quests = new List<Quest>();

    public bool HasQuest(Quest quest)
    {
        return Quests.Contains(quest);
    }

    public void AddQuest(Quest quest)
    {
        if(!HasQuest(quest)) 
        {
            Quests.Add(quest);
        }

        OnQuestPoolChanged?.Invoke();

    }

    public void DeleteQuest(Quest quest)
    {
        if (HasQuest(quest))
        {
            Quests.Remove(quest);
        }

        OnQuestPoolChanged?.Invoke();

    }

    public List<Quest> GetQuests()
    {
        return Quests;
    }
}
