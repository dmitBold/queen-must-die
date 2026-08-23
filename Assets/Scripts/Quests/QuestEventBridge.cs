using UnityEngine;
using Zenject;

public class QuestEventBridge : MonoBehaviour
{
    private QuestManager questManager;

    [Inject]
    public void Construct(QuestManager questManager)
    {
        this.questManager = questManager;
    }

    [Tooltip("Добавить новый квест в журнал")]
    public void AddQuest(Quest quest)
    {
        questManager.AddQuest(quest);
    }

    [Tooltip("Продвинуться по квесту (для линейных стадий)")]
    public void AdvanceQuest(Quest quest)
    {
        questManager.AdvanceQuest(quest);
    }
}