using System.Collections.Generic;
using System.Collections; // Обязательно добавляем для работы корутин
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class IconTurnOn : MonoBehaviour
{
    public List<Image> images;

    /// <summary>
    /// Группы, которые проявляются и гаснут вместе с images. Нужны для шкал с барабаном: его цифры создаются в рантайме и в images не попадают.
    /// </summary>
    [SerializeField] private List<CanvasGroup> canvasGroups = new List<CanvasGroup>();

    public UnityEvent onImagesFadein;
    public UnityEvent onImagesFadeout;

    // Храним текущую корутину, чтобы они не накладывались друг на друга
    private Coroutine fadeCoroutine;

    public void Start()
    {
        hide();
    }

    public void do_fade_in()
    {
        Debug.Log("ifadein");

        // Если что-то уже плавно исчезает или появляется, останавливаем это
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        // Запускаем корутину появления
        fadeCoroutine = StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        foreach (Image image in images)
        {
            if (image != null)
            {
                image.CrossFadeAlpha(1f, 2f, false);
            }
        }

        // Ждем ровно 2 секунды, пока идет анимация
        yield return FadeGroups(1f, 2f);

        // Только теперь вызываем ивент
        onImagesFadein?.Invoke();
    }

    public void do_fade_out()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        foreach (Image image in images)
        {
            if (image != null)
            {
                image.CrossFadeAlpha(0f, 2f, false);
            }
        }

        // Ждем ровно 2 секунды, пока идет анимация
        yield return FadeGroups(0f, 2f);

        // Только теперь вызываем ивент
        onImagesFadeout?.Invoke();
    }

    public void hide()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        foreach (Image image in images)
        {
            if (image != null)
            {
                image.canvasRenderer.SetAlpha(0f);
            }
        }
        SetGroupsAlpha(0f);
    }

    public void show()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        foreach (Image image in images)
        {
            if (image != null)
            {
                image.canvasRenderer.SetAlpha(1f);
            }
        }
        SetGroupsAlpha(1f);
    }

    private IEnumerator FadeGroups(float target, float duration)
    {
        var start = new float[canvasGroups.Count];
        for (int i = 0; i < canvasGroups.Count; i++)
            start[i] = canvasGroups[i] != null ? canvasGroups[i].alpha : 0f;

        for (float time = 0f; time < duration; time += Time.deltaTime)
        {
            for (int i = 0; i < canvasGroups.Count; i++)
                if (canvasGroups[i] != null)
                    canvasGroups[i].alpha = Mathf.Lerp(start[i], target, time / duration);
            yield return null;
        }

        SetGroupsAlpha(target);
    }

    private void SetGroupsAlpha(float alpha)
    {
        foreach (CanvasGroup group in canvasGroups)
        {
            if (group != null)
            {
                group.alpha = alpha;
            }
        }
    }
}
