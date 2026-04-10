using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImpactIcon : MonoBehaviour
{
    [SerializeField] WorldState.Stats stat;
    [SerializeField] Image icon;
    [SerializeField] CanvasGroup canvasGroup;

    //[Header("Scaling")]
    [SerializeField] bool allowScaling = true;
    [SerializeField] float smallScale = 0.25f;
    [SerializeField] float mediumScale = 0.35f;
    [SerializeField] float largeScale = 0.6f;

    [SerializeField] StatImpactVisualData visualData;

    //test
    [SerializeField] float animDuration = 0.2f;
    [SerializeField] Vector3 hiddenOffset = new Vector3(0, -20f, 0);
    Vector3 shownPos;
    Coroutine animRoutine;
    //test

    public WorldState.Stats Stat => stat;



    public void Setup(int value, ChoiceImpactUI.ImpactVisualMode mode)
    {
        StatImpactVisualData.ImpactSize size = GetSize(value);

        if (mode == ChoiceImpactUI.ImpactVisualMode.IconOnly ||
            mode == ChoiceImpactUI.ImpactVisualMode.IconAndScale)
        {
            icon.sprite = visualData.GetSprite(value, size);
        }

        if ((mode == ChoiceImpactUI.ImpactVisualMode.ScaleOnly ||
             mode == ChoiceImpactUI.ImpactVisualMode.IconAndScale)
            && allowScaling)
        {
            transform.localScale = GetScale(size);
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    StatImpactVisualData.ImpactSize GetSize(int value)
    {
        int abs = Mathf.Abs(value);

        if (abs <= 10) return StatImpactVisualData.ImpactSize.Small;
        if (abs <= 25) return StatImpactVisualData.ImpactSize.Medium;
        return StatImpactVisualData.ImpactSize.Large;
    }

    Vector3 GetScale(StatImpactVisualData.ImpactSize size)
    {
        return size switch
        {
            StatImpactVisualData.ImpactSize.Small => Vector3.one * smallScale,
            StatImpactVisualData.ImpactSize.Medium => Vector3.one * mediumScale,
            StatImpactVisualData.ImpactSize.Large => Vector3.one * largeScale,
            _ => Vector3.one
        };
    }

    public void Show()
    {
        //canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
        StartAnim(1f, shownPos, Vector3.one);
    }

    public void Hide()
    {
        StartAnim(0f, shownPos + hiddenOffset, transform.localScale);

        //canvasGroup.alpha = 0f;
    }

    public void HideImmediate()
    {

        if (animRoutine != null)
            StopCoroutine(animRoutine);

        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.one;

        transform.localPosition = shownPos + hiddenOffset;

    }

    public void Awake()
    {
        shownPos = transform.localPosition;
        HideImmediate();
    }

    //test
    void StartAnim(float targetAlpha, Vector3 targetPos, Vector3 targetScale)
    {
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(Animate(targetAlpha, targetPos, targetScale));
    }

    IEnumerator Animate(float targetAlpha, Vector3 targetPos, Vector3 targetScale)
    {
        float startAlpha = canvasGroup.alpha;
        Vector3 startPos = transform.localPosition;
        Vector3 startScale = transform.localScale;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / animDuration;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            //transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        transform.localPosition = targetPos;
        //transform.localScale = targetScale;
    }
    //test


}
