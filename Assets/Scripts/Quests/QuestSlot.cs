using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Слот квеста в списке журнала. Шапка: иконка, название и стрелка раскрытия; под ней всегда
/// текущий этап. Раскрытие по стрелке показывает выполненные этапы (новые сверху), клик по слоту
/// выбирает квест. Иконка, этапы и прочие данные квеста необязательны.
/// </summary>
public class QuestSlot : MonoBehaviour
{
    // Стрелка в раскрытом состоянии повёрнута на 90° по часовой относительно свёрнутого (вправо → вниз).
    private const float ExpandedArrowRotation = -90f;

    [Header("Header")]
    [Tooltip("Фон слота: подсвечивается, когда квест выбран. Кнопка выбора висит на корне слота.")]
    [SerializeField] private Image background;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button arrowButton;
    [Tooltip("Картинка стрелки. Должна быть общей для всех квестов и задаваться в префабе.")]
    [SerializeField] private Image arrowImage;

    [Header("Stages")]
    [SerializeField] private TextMeshProUGUI currentStageText;
    [Tooltip("LayoutElement контейнера выполненных этапов: его высота анимируется при раскрытии.")]
    [SerializeField] private LayoutElement completedLayout;
    [Tooltip("Содержимое контейнера выполненных этапов: вертикальная группа, сюда создаются строки.")]
    [SerializeField] private RectTransform completedContent;
    [SerializeField] private QuestStageRow stageRowPrefab;

    [Header("Look")]
    [SerializeField] private Color normalColor = new Color(0.15f, 0.15f, 0.15f, 0.6f);
    [SerializeField] private Color selectedColor = new Color(0.4f, 0.32f, 0.18f, 0.9f);
    [Tooltip("Угол Z стрелки в свёрнутом состоянии. В раскрытом она повёрнута ещё на -90°: "
        + "для спрайта, смотрящего вправо — 0, для спрайта, смотрящего вниз — 90.")]
    [SerializeField] private float arrowCollapsedAngle;
    [Tooltip("Длительность полного раскрытия или сворачивания, секунды.")]
    [SerializeField] private float expandDuration = 0.25f;
    [SerializeField] private AnimationCurve expandCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private readonly List<QuestStageRow> _rows = new();

    private QuestProgress _progress;
    private bool _isExpanded;
    private float _expandProgress;
    private Coroutine _expandRoutine;

    /// <summary>
    /// Вызывается при клике по слоту (кроме клика по стрелке).
    /// </summary>
    public event Action<QuestSlot> Selected;

    /// <summary>
    /// Квест, который показывает слот. Null, пока слот не настроен.
    /// </summary>
    public Quest Quest => _progress?.Quest;

    private void Awake()
    {
        selectButton.onClick.AddListener(HandleSelectClicked);
        arrowButton.onClick.AddListener(ToggleExpanded);
    }

    private void OnEnable()
    {
        // Раскладка неактивного слота не считается, поэтому высоту раскрытого слота выставляем при включении.
        SnapExpansion();
    }

    private void OnDisable()
    {
        // Unity сам останавливает корутину при отключении; сбрасываем ссылку и незавершённую анимацию.
        _expandRoutine = null;
        _expandProgress = _isExpanded ? 1f : 0f;
    }

    /// <summary>
    /// Обновляет слот по прогрессу квеста. Состояние раскрытия сохраняется, пока есть что раскрывать.
    /// </summary>
    public void Set(QuestProgress progress)
    {
        _progress = progress;
        Quest quest = progress.Quest;
        bool isCompleted = progress.IsCompleted;

        // Image выключается, а не деактивируется: слот без иконки сохраняет выравнивание названия.
        iconImage.sprite = quest.Icon;
        iconImage.enabled = quest.Icon != null;

        nameText.text = quest.QuestName;
        nameText.fontStyle = WithStrikethrough(nameText.fontStyle, isCompleted);

        string currentStage = isCompleted ? null : progress.GetCurrentStage()?.StageDescription;
        currentStageText.text = currentStage ?? string.Empty;
        currentStageText.gameObject.SetActive(!string.IsNullOrEmpty(currentStage));

        bool hasCompletedStages = RebuildCompletedRows(progress);
        arrowImage.enabled = hasCompletedStages;
        arrowButton.interactable = hasCompletedStages;
        if (!hasCompletedStages)
        {
            _isExpanded = false;
        }

        // Данные могли измениться посреди анимации, поэтому сразу приводим слот к итоговому состоянию.
        StopExpansion();
        if (isActiveAndEnabled)
        {
            SnapExpansion();
        }
        else
        {
            _expandProgress = _isExpanded ? 1f : 0f;
        }
    }

    /// <summary>
    /// Включает или выключает подсветку выбранного слота.
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        background.color = isSelected ? selectedColor : normalColor;
    }

    private void HandleSelectClicked()
    {
        Selected?.Invoke(this);
    }

    private void ToggleExpanded()
    {
        _isExpanded = !_isExpanded;
        StopExpansion();
        _expandRoutine = StartCoroutine(AnimateExpansion());
    }

    // Выполненные этапы идут от последнего к первому. У завершённого квеста выполнены все этапы,
    // у активного — все до текущего (текущий показывается отдельно, без зачёркивания).
    private bool RebuildCompletedRows(QuestProgress progress)
    {
        List<QuestStage> stages = progress.Quest.QuestStages;
        int stageCount = stages != null ? stages.Count : 0;
        int completedCount = progress.IsCompleted
            ? stageCount
            : Mathf.Clamp(progress.CurrentStageIndex, 0, stageCount);

        for (int i = 0; i < completedCount; i++)
        {
            QuestStageRow row = GetRow(i);
            row.gameObject.SetActive(true);
            row.Set(stages[completedCount - 1 - i].StageDescription);
        }

        for (int i = completedCount; i < _rows.Count; i++)
        {
            _rows[i].gameObject.SetActive(false);
        }

        return completedCount > 0;
    }

    private QuestStageRow GetRow(int index)
    {
        while (_rows.Count <= index)
        {
            _rows.Add(Instantiate(stageRowPrefab, completedContent));
        }

        return _rows[index];
    }

    private IEnumerator AnimateExpansion()
    {
        float from = _expandProgress;
        float to = _isExpanded ? 1f : 0f;
        float fullHeight = MeasureCompletedHeight();
        float duration = expandDuration * Mathf.Abs(to - from);

        // unscaledDeltaTime: журнал должен анимироваться и при остановленном timeScale.
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            _expandProgress = Mathf.Lerp(from, to, elapsed / duration);
            ApplyExpansion(fullHeight);
            yield return null;
        }

        _expandProgress = to;
        ApplyExpansion(fullHeight);
        _expandRoutine = null;
    }

    private void SnapExpansion()
    {
        _expandProgress = _isExpanded ? 1f : 0f;
        ApplyExpansion(_isExpanded ? MeasureCompletedHeight() : 0f);
    }

    private void ApplyExpansion(float fullHeight)
    {
        float eased = expandCurve.Evaluate(_expandProgress);
        completedLayout.preferredHeight = fullHeight * eased;

        float angle = Mathf.LerpUnclamped(arrowCollapsedAngle, arrowCollapsedAngle + ExpandedArrowRotation, eased);
        arrowImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    // Высота считается по контенту, а не по контейнеру: у контейнера высота принудительно
    // задана через LayoutElement и размер строк не отражает.
    private float MeasureCompletedHeight()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(completedContent);
        return LayoutUtility.GetPreferredHeight(completedContent);
    }

    private void StopExpansion()
    {
        if (_expandRoutine == null)
        {
            return;
        }

        StopCoroutine(_expandRoutine);
        _expandRoutine = null;
    }

    private static FontStyles WithStrikethrough(FontStyles style, bool isEnabled)
    {
        return isEnabled ? style | FontStyles.Strikethrough : style & ~FontStyles.Strikethrough;
    }
}
