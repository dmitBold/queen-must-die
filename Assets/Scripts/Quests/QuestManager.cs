using System;
using System.Collections.Generic;

// Класс для хранения рантайм-состояния квеста
public class QuestProgress
{
    public Quest Quest { get; private set; }
    public int CurrentStageIndex { get; set; }
    public bool IsCompleted { get; set; }

    public QuestProgress(Quest quest)
    {
        Quest = quest;
        CurrentStageIndex = 0;
        IsCompleted = false;
    }

    public QuestStage GetCurrentStage()
    {
        if (Quest.QuestStages != null && CurrentStageIndex < Quest.QuestStages.Count)
        {
            return Quest.QuestStages[CurrentStageIndex];
        }
        return null;
    }
}

public class QuestManager
{
    public event Action OnQuestPoolChanged;

    // C#-события для архитектурной отвязки от сцены
    public event Action<Quest, int> OnQuestStageFinished;
    public event Action<Quest> OnQuestFinished;

    private List<QuestProgress> activeQuests = new List<QuestProgress>();

    public bool HasQuest(Quest quest)
    {
        return GetQuestProgress(quest) != null;
    }

    public void AddQuest(Quest quest)
    {
        if (!HasQuest(quest))
        {
            activeQuests.Add(new QuestProgress(quest));
            OnQuestPoolChanged?.Invoke();
        }
    }

    public void DeleteQuest(Quest quest)
    {
        var progress = GetQuestProgress(quest);
        if (progress != null)
        {
            activeQuests.Remove(progress);
            OnQuestPoolChanged?.Invoke();
        }
    }

    public List<QuestProgress> GetActiveQuests()
    {
        return activeQuests;
    }

    private QuestProgress GetQuestProgress(Quest quest)
    {
        return activeQuests.Find(q => q.Quest == quest);
    }

    // Главный метод для продвижения по квесту (вызывай его из триггеров/диалогов)
    public void AdvanceQuest(Quest quest)
    {
        var progress = GetQuestProgress(quest);
        if (progress == null || progress.IsCompleted) return;

        int finishedStageIndex = progress.CurrentStageIndex;

        // Вызываем событие о завершении конкретного этапа
        OnQuestStageFinished?.Invoke(quest, finishedStageIndex);

        if (progress.CurrentStageIndex < progress.Quest.QuestStages.Count - 1)
        {
            progress.CurrentStageIndex++;
            OnQuestPoolChanged?.Invoke();
        }
        else
        {
            progress.IsCompleted = true;
            OnQuestFinished?.Invoke(quest); // Вызываем событие о завершении квеста
            OnQuestPoolChanged?.Invoke();
        }
    }
}