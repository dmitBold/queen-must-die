using TMPro;
using UnityEngine;

/// <summary>
/// Строка выполненного этапа в раскрытом слоте журнала: зачёркнутый текст.
/// Иконка «выполнено» общая для всех квестов и задаётся в префабе строки.
/// </summary>
public class QuestStageRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    /// <summary>
    /// Показывает описание этапа зачёркнутым текстом.
    /// </summary>
    public void Set(string stageDescription)
    {
        text.text = stageDescription;
        text.fontStyle |= FontStyles.Strikethrough;
    }
}
