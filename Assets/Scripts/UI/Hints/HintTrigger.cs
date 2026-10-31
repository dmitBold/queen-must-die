using UnityEngine;
using Zenject;

public class HintTrigger : MonoBehaviour
{
    [TextArea(2, 5)]
    [Tooltip("Текст подсказки, который появится на экране")]
    public string hintText;

    [Header("Тайминги для этой конкретной подсказки")]
    [Tooltip("Сколько времени подсказка висит на экране (сек)")]
    public float displayDuration = 3.0f;
    [Tooltip("Скорость появления и исчезновения (сек)")]
    public float fadeDuration = 0.5f;

    [Tooltip("Показывать подсказку сразу при старте сцены?")]
    public bool showOnStart = false;

    private HintManager _hintManager;

    [Inject]
    public void Construct(HintManager hintManager)
    {
        _hintManager = hintManager;
    }

    private void Start()
    {
        if (showOnStart)
        {
            TriggerHint();
        }
    }

    // Вызывай этот метод через UnityEvent из инспектора!
    public void TriggerHint()
    {
        if (_hintManager != null && !string.IsNullOrEmpty(hintText))
        {
            _hintManager.ShowHint(hintText, displayDuration, fadeDuration);
        }
        else
        {
            Debug.LogWarning("[HintTrigger] Текст пуст или HintManager не заинжекчен.");
        }
    }
}