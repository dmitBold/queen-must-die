using Cards;
using Core;
using UnityEngine;
using Zenject;

/// <summary>
/// Квест «Запретная археология» в King block Day. Выдаётся после разговора с Ромом (GiveQuest из onDialogueCompleted).
/// Второй этап наступает после любого решения по ExcavationCard2: оба выбора ставят флаг, и компонент переносит его
/// в общий WorldState, поэтому если решение принято до разговора с Ромом, квест выдаётся сразу со второго этапа.
/// Третий этап наступает при прочтении любого из писем в пещерах (OnLetterRead). Этапы только растут.
/// </summary>
public class ForbiddenArchaeologyQuest : MonoBehaviour
{
    private const int FindCavesStage = 1;

    /// <summary>Ассет квеста «Запретная археология».</summary>
    [SerializeField] private Quest _quest;

    /// <summary>CardManager доков, в которых принимается решение по ExcavationCard2.</summary>
    [SerializeField] private CardManager _cardManager;

    /// <summary>
    /// StateManager, в который пишет CardManager доков (его поле World State Manual). В King block Day доки
    /// намеренно работают с локальным состоянием сцены, а не с общим WorldState.
    /// </summary>
    [SerializeField] private WorldState _docsWorldState;

    /// <summary>Флаг, который ставят оба выбора ExcavationCard2 (поле Add Flags). Должен совпадать с картой.</summary>
    [SerializeField] private string _excavationDecidedFlag = "quest:forbidden_archaeology:caves";

    private QuestManager _quests;
    private WorldState _worldState;

    [Inject]
    public void Construct(QuestManager questManager, WorldState worldState)
    {
        _quests = questManager;
        _worldState = worldState;
    }

    private void OnEnable()
    {
        if (_cardManager != null) _cardManager.OnAnyChoiceResolved += OnChoiceResolved;
        else Debug.LogError($"{name}: не назначен CardManager, второй этап квеста не включится по решению в доках.", this);

        if (_docsWorldState == null)
            Debug.LogError($"{name}: не назначен StateManager доков, решение по карте не будет учтено.", this);
    }

    private void OnDisable()
    {
        if (_cardManager != null) _cardManager.OnAnyChoiceResolved -= OnChoiceResolved;
    }

    /// <summary>Выдаёт квест; вызывается из onDialogueCompleted Рома.</summary>
    public void GiveQuest()
    {
        if (_quests == null || _quest == null || _quests.HasQuest(_quest)) return;

        _quests.AddQuest(_quest, IsExcavationDecided() ? FindCavesStage : 0);
    }

    /// <summary>
    /// Переводит квест с этапа «Найдите путь в пещеры» на следующий; вызывается из noteEvent писем в пещерах.
    /// На других этапах ничего не делает, поэтому второе и повторное прочтение безопасны.
    /// </summary>
    public void OnLetterRead()
    {
        if (_quests == null || _quest == null) return;

        QuestProgress progress = _quests.GetActiveQuests().Find(q => q.Quest == _quest);
        if (progress == null || progress.IsCompleted || progress.CurrentStageIndex != FindCavesStage) return;

        _quests.AdvanceQuest(_quest);
    }

    private void OnChoiceResolved()
    {
        // Локальное состояние доков не сохраняется, поэтому решение переносится в общий WorldState:
        // по нему квест выдаётся сразу со второго этапа, в том числе после загрузки.
        if (_docsWorldState != null && _worldState != null && _docsWorldState.HasFlag(_excavationDecidedFlag))
        {
            _worldState.AddFlag(_excavationDecidedFlag);
        }

        if (_quests == null || _quest == null || !IsExcavationDecided()) return;

        QuestProgress progress = _quests.GetActiveQuests().Find(q => q.Quest == _quest);
        if (progress == null || progress.IsCompleted || progress.CurrentStageIndex >= FindCavesStage) return;

        _quests.AdvanceQuest(_quest);
    }

    private bool IsExcavationDecided()
    {
        return _worldState != null && _worldState.HasFlag(_excavationDecidedFlag);
    }
}
