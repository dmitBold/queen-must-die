using Core;
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

    /// <summary>Флаг WorldState, по которому подсказка показывается один раз за прохождение. Пусто: без ограничения.</summary>
    [Tooltip("Если задан, подсказка покажется один раз за прохождение (флаг сохраняется в WorldState)")]
    [SerializeField] private string showOnceFlag;

    private HintManager _hintManager;
    private WorldState _worldState;

    [Inject]
    public void Construct(HintManager hintManager, WorldState worldState)
    {
        _hintManager = hintManager;
        _worldState = worldState;
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
            if (!string.IsNullOrEmpty(showOnceFlag))
            {
                if (_worldState.HasFlag(showOnceFlag)) return;
                _worldState.AddFlag(showOnceFlag);
            }

            _hintManager.ShowHint(hintText, displayDuration, fadeDuration);
        }
        else
        {
            Debug.LogWarning("[HintTrigger] Текст пуст или HintManager не заинжекчен.");
        }
    }
}