using System.Collections;
using TMPro;
using UnityEngine;

public class FlashlightUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    public TextMeshProUGUI TMPtext;
    [SerializeField] private Canvas rootCanvas;

    [Header("State")]
    public bool isOpen = false;
    public bool IsFading = false;

    public void DisableRootCanvas() => rootCanvas.gameObject.SetActive(false);
    public void EnableRootCanvas() => rootCanvas.gameObject.SetActive(true);

    public void Open()
    {
        isOpen = true;
        panel.SetActive(true);
    }

    public void Close()
    {
        isOpen = false;
        panel.SetActive(false);
    }

    public void SetText(string text)
    {
        TMPtext.text = text;
    }

    public IEnumerator FadeTo(float targetAlpha, float duration)
    {
        IsFading = true;
        Color startColor = TMPtext.color;
        float startAlpha = startColor.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            TMPtext.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        TMPtext.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        IsFading = false;
    }

    public void ResetAlpha()
    {
        Color c = TMPtext.color;
        c.a = 1f;
        TMPtext.color = c;
        IsFading = false;
    }
}