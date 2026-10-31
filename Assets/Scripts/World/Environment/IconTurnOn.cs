using System.Collections.Generic;
using System.Collections; // Обязательно добавляем для работы корутин
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class IconTurnOn : MonoBehaviour
{
    public List<Image> images;

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
        yield return new WaitForSeconds(2f);

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
        yield return new WaitForSeconds(2f);

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
    }
}
