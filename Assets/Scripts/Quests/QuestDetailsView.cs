using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Правая панель журнала квестов: арт выбранного квеста и прокручиваемое описание.
/// Любое из полей квеста может отсутствовать: тогда соответствующая область остаётся пустой,
/// а сама раскладка панели не меняется.
/// </summary>
public class QuestDetailsView : MonoBehaviour
{
    [SerializeField] private Image artImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private ScrollRect descriptionScroll;

    private Quest _shownQuest;
    private string _shownDescription;

    /// <summary>
    /// Показывает арт и актуальное для текущего этапа описание квеста. Если квест и описание не изменились,
    /// панель не трогается, чтобы не сбрасывать прокрутку при несвязанных обновлениях журнала.
    /// </summary>
    public void Show(QuestProgress progress)
    {
        Quest quest = progress.Quest;

        // У завершённого квеста индекс остаётся на последнем этапе, поэтому его флаг тоже учитывается.
        int stageIndex = progress.IsCompleted ? quest.QuestStages.Count - 1 : progress.CurrentStageIndex;
        string description = quest.GetDescription(stageIndex) ?? string.Empty;

        if (quest == _shownQuest && description == _shownDescription)
        {
            return;
        }

        _shownQuest = quest;
        _shownDescription = description;
        Apply(quest.ArtSprite, description);
    }

    /// <summary>
    /// Очищает панель: ни арта, ни описания.
    /// </summary>
    public void Clear()
    {
        _shownQuest = null;
        _shownDescription = null;
        Apply(null, string.Empty);
    }

    private void Apply(Sprite art, string description)
    {
        // Image выключается, а не деактивируется: пустая область сохраняет свой размер в раскладке.
        artImage.sprite = art;
        artImage.enabled = art != null;

        descriptionText.text = description ?? string.Empty;

        // Без принудительной пересборки позиция сбрасывается по размеру контента предыдущего квеста.
        LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionScroll.content);
        descriptionScroll.velocity = Vector2.zero;
        descriptionScroll.verticalNormalizedPosition = 1f;
    }
}
