using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestStage
{
    [TextArea(3, 5)]
    public string StageDescription;
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
}