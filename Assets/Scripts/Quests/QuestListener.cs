using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Zenject;

public class QuestListener : MonoBehaviour
{
    [Tooltip("Квест, который мы отслеживаем")]
    [SerializeField] private Quest targetQuest;

    [Header("Событие завершения всего квеста")]
    public UnityEvent OnQuestCompleted;

    [Header("События завершения отдельных этапов")]
    [Tooltip("Настрой, какие события произойдут при завершении определенных этапов (индекс начинается с 0)")]
    public List<StageEvent> stageEvents = new List<StageEvent>();

    [System.Serializable]
    public class StageEvent
    {
        public int stageIndex;
        public UnityEvent onStageFinished;
    }

    private QuestManager questManager;

    [Inject]
    public void Construct(QuestManager questManager)
    {
        this.questManager = questManager;

        // Подписываемся на события менеджера
        questManager.OnQuestFinished += HandleQuestFinished;
        questManager.OnQuestStageFinished += HandleQuestStageFinished;
    }

    private void OnDestroy()
    {
        if (questManager != null)
        {
            questManager.OnQuestFinished -= HandleQuestFinished;
            questManager.OnQuestStageFinished -= HandleQuestStageFinished;
        }
    }

    private void HandleQuestFinished(Quest quest)
    {
        if (quest == targetQuest)
        {
            OnQuestCompleted?.Invoke();
        }
    }

    private void HandleQuestStageFinished(Quest quest, int stageIndex)
    {
        if (quest == targetQuest)
        {
            foreach (var stageEvent in stageEvents)
            {
                if (stageEvent.stageIndex == stageIndex)
                {
                    stageEvent.onStageFinished?.Invoke();
                }
            }
        }
    }
}