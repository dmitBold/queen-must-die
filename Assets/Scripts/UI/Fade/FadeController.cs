using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

//[RequireComponent(typeof(CanvasGroup))]
public class FadeController : MonoBehaviour
{
    public static FadeController Instance { get; private set; }
    private CanvasGroup canvasGroup;
    /*public UnityEvent onfadedIN;
    public UnityEvent onfadedOUT;*/


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeToBlack(float duration, Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(DoFade(canvasGroup.alpha, 1f, duration, onComplete));
    }

    public void FadeToClear(float duration, Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(DoFade(canvasGroup.alpha, 0f, duration, onComplete));
    }

    public void FadeToBlack_simple(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(DoFade(canvasGroup.alpha, 1f, duration, null));
    }

    public void FadeToClear_simple(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(DoFade(canvasGroup.alpha, 0f, duration, null));
    }

    private IEnumerator DoFade(float startAlpha, float endAlpha, float duration, Action onComplete)
    {
        float time = 0;
        canvasGroup.blocksRaycasts = true;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        canvasGroup.blocksRaycasts = endAlpha > 0;
        /*if (endAlpha == 0f)
        {
            onfadedOUT?.Invoke();
        }
        else if (endAlpha == 1f)
        {
            onfadedIN?.Invoke();
        }*/
            onComplete?.Invoke();
    }

    public void SetAlphaDirectly(float alpha)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.blocksRaycasts = alpha > 0;
    }
}