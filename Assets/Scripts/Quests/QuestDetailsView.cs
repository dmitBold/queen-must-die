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

    /// <summary>
    /// Показывает арт и описание квеста.
    /// </summary>
    public void Show(Quest quest)
    {
        Apply(quest.ArtSprite, quest.Description);
    }

    /// <summary>
    /// Очищает панель: ни арта, ни описания.
    /// </summary>
    public void Clear()
    {
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
