using System.Collections;
using UnityEngine;
using Zenject;

/// <summary>
/// Квест «Ночь тайн» на ночной сцене.
/// 0 — Найдите дверь под руками (выдаётся при старте ночи, если квеста ещё нет);
/// 1 — Найдите способ открыть дверь (взаимодействие с дверью у сломанного голема);
/// 2 — Верните части тела голема (диалог с големом; если голем раньше двери — сразу сюда);
/// 3 — СПАСИТЕСЬ (голем собран).
/// Этапы только растут: повторные события и события «из прошлого» ничего не откатывают.
/// </summary>
public class NightSecretsQuest : MonoBehaviour
{
    public Quest quest;
    [Tooltip("Выдать квест при старте сцены, если его ещё нет")]
    public bool giveOnStart = true;
    /// <summary>Задержка выдачи квеста после старта сцены, в секундах.</summary>
    [SerializeField] private float giveDelay = 1f;

    public const int FindDoor = 0, OpenDoor = 1, ReturnParts = 2, Escape = 3;

    private QuestManager _quests;

    [Inject]
    public void Construct(QuestManager questManager) => _quests = questManager;

    private IEnumerator Start()
    {
        if (!giveOnStart) yield break;
        yield return new WaitForSeconds(giveDelay);
        // Проверка после паузы: за это время квест могли выдать через AdvanceTo
        if (_quests != null && quest != null && !_quests.HasQuest(quest)) _quests.AddQuest(quest);
    }

    public void OnDoorReached() => AdvanceTo(OpenDoor);
    public void OnGolemExamined() => AdvanceTo(ReturnParts);
    public void OnGolemAssembled() => AdvanceTo(Escape);

    private void AdvanceTo(int stage)
    {
        if (_quests == null || quest == null) return;
        if (!_quests.HasQuest(quest)) _quests.AddQuest(quest);
        var progress = _quests.GetActiveQuests().Find(q => q.Quest == quest);
        if (progress == null) return;
        int guard = 10;
        while (!progress.IsCompleted && progress.CurrentStageIndex < stage && guard-- > 0) _quests.AdvanceQuest(quest);
    }
}
