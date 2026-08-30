using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public struct HintData
{
    public string Text;
    public float DisplayDuration;
    public float FadeDuration;
}

public class HintManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup hintCanvasGroup;
    [SerializeField] private TMP_Text hintText;

    [Header("Глобальные настройки")]
    [Tooltip("Пауза между уходом старой подсказки и появлением новой")]
    [SerializeField] private float delayBetweenHints = 0.5f;

    private Queue<HintData> hintQueue = new Queue<HintData>();
    private bool isProcessingQueue = false;

    private void Start()
    {
        if (hintCanvasGroup != null)
        {
            hintCanvasGroup.alpha = 0f;
            hintText.text = "";
        }
    }

    // метод принимает индивидуальные настройки времени
    public void ShowHint(string text, float displayDuration, float fadeDuration)
    {
        //Debug.Log(text);

        hintQueue.Enqueue(new HintData
        {
            Text = text,
            DisplayDuration = displayDuration,
            FadeDuration = fadeDuration
        });

        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        isProcessingQueue = true;

        while (hintQueue.Count > 0)
        {
            HintData currentHint = hintQueue.Dequeue();
            hintText.text = currentHint.Text;

            // 1. Fade In
            yield return StartCoroutine(FadeTo(1f, currentHint.FadeDuration));

            // 2. Держим на экране
            yield return new WaitForSeconds(currentHint.DisplayDuration);

            // 3. Fade Out
            yield return StartCoroutine(FadeTo(0f, currentHint.FadeDuration));

            // 4. Пауза перед следующей подсказкой
            yield return new WaitForSeconds(delayBetweenHints);

            hintText.text = ""; // Очищаем текст, пока ждем
        }

        isProcessingQueue = false;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (hintCanvasGroup == null) yield break;

        float startAlpha = hintCanvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            hintCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        hintCanvasGroup.alpha = targetAlpha;
    }
}