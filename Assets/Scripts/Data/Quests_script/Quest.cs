using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestStage
{
    [TextArea(3, 5)]
    public string StageDescription;

    [Tooltip("Если включено, при входе на этот этап описание квеста в журнале заменяется текстом ниже. "
        + "Пустой текст очищает описание.")]
    [SerializeField] private bool updatesQuestDescription;

    [Tooltip("Новое описание квеста. Применяется, только если включён флаг обновления описания.")]
    [TextArea(3, 10)]
    [SerializeField] private string questDescription;

    /// <summary>
    /// Истина, если этот этап меняет описание квеста в журнале.
    /// </summary>
    public bool UpdatesQuestDescription => updatesQuestDescription;

    /// <summary>
    /// Описание квеста, которое устанавливает этап. Может быть пустым: тогда описание очищается.
    /// </summary>
    public string QuestDescription => questDescription;
}

[CreateAssetMenu(fileName = "Quest", menuName = "Scriptable Objects/Quest")]
public class Quest : ScriptableObject
{
    [Header("Base Info")]
    public string id;

    [TextArea(3, 5)]
    public string QuestName;

    public List<QuestStage> QuestStages;

    [Header("Journal (optional)")]
    [Tooltip("Основной квест попадает в секцию «Основные», остальные — в «Побочные».")]
    [SerializeField] private bool isMain;

    [Tooltip("Уникальная иконка квеста в слоте журнала. Можно не задавать.")]
    [SerializeField] private Sprite icon;

    [Tooltip("Арт квеста в правой панели журнала. Можно не задавать.")]
    [SerializeField] private Sprite artSprite;

    [Tooltip("Описание квеста в правой панели журнала. Можно не задавать.")]
    [TextArea(3, 10)]
    [SerializeField] private string description;

    /// <summary>
    /// Истина для основного квеста, ложь для побочного.
    /// </summary>
    public bool IsMain => isMain;

    /// <summary>
    /// Иконка квеста для слота журнала. Может быть null.
    /// </summary>
    public Sprite Icon => icon;

    /// <summary>
    /// Арт квеста для правой панели журнала. Может быть null.
    /// </summary>
    public Sprite ArtSprite => artSprite;

    /// <summary>
    /// Описание квеста для правой панели журнала. Может быть пустым.
    /// </summary>
    public string Description => description;

    /// <summary>
    /// Описание квеста на указанном этапе: берётся у ближайшего этапа с флагом обновления описания,
    /// начиная с текущего и назад к первому; если такого нет — базовое описание. Пустой текст этапа очищает описание.
    /// </summary>
    public string GetDescription(int currentStageIndex)
    {
        if (QuestStages != null)
        {
            for (int i = Mathf.Min(currentStageIndex, QuestStages.Count - 1); i >= 0; i--)
            {
                if (QuestStages[i].UpdatesQuestDescription)
                {
                    return QuestStages[i].QuestDescription ?? string.Empty;
                }
            }
        }

        return description;
    }
}