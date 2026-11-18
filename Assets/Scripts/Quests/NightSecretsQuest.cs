using System.Collections;
using Core;
using UnityEngine;
using Zenject;

/// <summary>
/// Квест «Ночь тайн» на ночной сцене.
/// 0 — Сбегите из замка от неизвестной угрозы (выдаётся при старте ночи, если квеста ещё нет);
/// 1 — Откройте ворота из королевской башни (взаимодействие с дверью у сломанного голема или диалог с големом);
/// 2 — СПАСИТЕСЬ (голем собран).
/// Этапы только растут: повторные события и события «из прошлого» ничего не откатывают.
/// </summary>
public class NightSecretsQuest : MonoBehaviour
{
    public Quest quest;
    [Tooltip("Выдать квест при старте сцены, если его ещё нет")]
    public bool giveOnStart = true;
    /// <summary>Задержка выдачи квеста после старта сцены, в секундах.</summary>
    [SerializeField] private float giveDelay = 1f;

    /// <summary>Хинт про журнал задач: один раз за прохождение, когда квест дошёл до этапа «Откройте ворота».</summary>
    [SerializeField] private string journalHintText = "Нажмите Q, чтобы открыть журнал задач";
    [SerializeField] private float journalHintDuration = 4f;
    [SerializeField] private float journalHintFade = 0.5f;
    /// <summary>Пауза перед хинтом, в секундах: чтобы он не накладывался на уведомление о смене этапа квеста.</summary>
    [SerializeField] private float journalHintDelay = 7f;

    public const int FindDoor = 0, OpenDoor = 1, Escape = 2;

    private const string JournalHintFlag = "hint:quest_journal";

    private QuestManager _quests;
    private HintManager _hints;
    private WorldState _worldState;

    [Inject]
    public void Construct(QuestManager questManager, HintManager hintManager, WorldState worldState)
    {
        _quests = questManager;
        _hints = hintManager;
        _worldState = worldState;
    }

    private IEnumerator Start()
    {
        if (!giveOnStart) yield break;
        yield return new WaitForSeconds(giveDelay);
        // Проверка после паузы: за это время квест могли выдать через AdvanceTo
        if (_quests != null && quest != null && !_quests.HasQuest(quest)) _quests.AddQuest(quest);
    }

    public void OnDoorReached() => AdvanceTo(OpenDoor);
    public void OnGolemExamined() => AdvanceTo(OpenDoor);
    public void OnGolemAssembled() => AdvanceTo(Escape);

    private void AdvanceTo(int stage)
    {
        if (_quests == null || quest == null) return;
        if (!_quests.HasQuest(quest)) _quests.AddQuest(quest);
        var progress = _quests.GetActiveQuests().Find(q => q.Quest == quest);
        if (progress == null) return;
        int before = progress.CurrentStageIndex;
        int guard = 10;
        while (!progress.IsCompleted && progress.CurrentStageIndex < stage && guard-- > 0) _quests.AdvanceQuest(quest);
        if (before < OpenDoor && progress.CurrentStageIndex >= OpenDoor) StartCoroutine(ShowJournalHint());
    }

    private IEnumerator ShowJournalHint()
    {
        yield return new WaitForSeconds(journalHintDelay);
        // Флаг ставится только при показе: если сцену выгрузят во время паузы, хинт покажется в следующий раз.
        if (_hints == null || _worldState == null || _worldState.HasFlag(JournalHintFlag)) yield break;
        _worldState.AddFlag(JournalHintFlag);
        _hints.ShowHint(journalHintText, journalHintDuration, journalHintFade);
    }
}
