using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Триггер-зона, которая вызывает UnityEvent, если игрок непрерывно находится внутри
/// заданное время. При выходе игрока отсчёт сбрасывается и событие может сработать снова.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TimedPlayerTrigger : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Тег объекта, который считается игроком.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Timing")]
    [Tooltip("Сколько секунд игрок должен непрерывно находиться внутри.")]
    [SerializeField, Min(0f)] private float requiredStayDuration = 3f;

    [Tooltip("Игнорировать Time.timeScale (полезно для пауз).")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Events")]
    [Tooltip("Вызывается один раз, когда игрок пробыл внутри нужное время.")]
    [SerializeField] private UnityEvent onPlayerStayed;

    private readonly HashSet<Collider> _collidersInside = new HashSet<Collider>();
    private float _timer;
    private bool _fired;

    private void Reset()
    {
        // При добавлении скрипта в редакторе автоматически делаем коллайдер триггером.
        if (TryGetComponent<Collider>(out var col))
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        _collidersInside.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_collidersInside.Remove(other))
            return;

        // Сброс только когда все коллайдеры игрока покинули триггер.
        if (_collidersInside.Count == 0)
            ResetState();
    }

    private void Update()
    {
        // Страховка от "зависших" ссылок (объект уничтожен, но OnTriggerExit не пришёл).
        _collidersInside.RemoveWhere(c => c == null);

        if (_fired || _collidersInside.Count == 0)
            return;

        _timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (_timer >= requiredStayDuration)
        {
            _fired = true;
            onPlayerStayed?.Invoke();
        }
    }

    private void OnDisable()
    {
        // Сбрасываем состояние, чтобы при повторном включении отсчёт начался с нуля.
        ResetState();
    }

    /// <summary>Принудительно сбросить отсчёт и разрешить повторное срабатывание.</summary>
    public void ResetState()
    {
        _collidersInside.Clear();
        _timer = 0f;
        _fired = false;
    }
}