using System;
using System.Collections.Generic;
using Zenject;

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

    public event Action<Quest> OnQuestAdded;

    // C#-события для архитектурной отвязки от сцены
    public event Action<Quest, int> OnQuestStageFinished;
    public event Action<Quest> OnQuestFinished;

    private List<QuestProgress> activeQuests = new List<QuestProgress>();

    [Inject] private QuestDatabase _questDatabase;

    //test

    public List<SaveManager.SavedQuest> GetQuestSaveData()
    {
        List<SaveManager.SavedQuest> savedList = new List<SaveManager.SavedQuest>();

        foreach (var progress in activeQuests)
        {
            if (progress.Quest != null && !string.IsNullOrEmpty(progress.Quest.id))
            {
                savedList.Add(new SaveManager.SavedQuest(
                    progress.Quest.id,
                    progress.CurrentStageIndex,
                    progress.IsCompleted
                ));
            }
        }

        return savedList;
    }

    public void LoadQuestData(List<SaveManager.SavedQuest> savedQuests)
    {
        activeQuests.Clear();

        if (savedQuests == null) return;

        foreach (var savedQuest in savedQuests)
        {
            Quest originalQuest = _questDatabase.GetQuestById(savedQuest.questId);

            if (originalQuest != null)
            {
                // Восстанавливаем состояние квеста
                QuestProgress progress = new QuestProgress(originalQuest)
                {
                    CurrentStageIndex = savedQuest.currentStageIndex,
                    IsCompleted = savedQuest.isCompleted
                };

                activeQuests.Add(progress);
            }
        }

        OnQuestPoolChanged?.Invoke(); // Оповещаем UI, что квесты обновились
    }

    //test


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
            OnQuestAdded?.Invoke(quest);
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

    // Главный метод для продвижения по квесту
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