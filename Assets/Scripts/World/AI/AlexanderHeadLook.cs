using NightCycle;
using UnityEngine;
using Zenject;

/// <summary>
/// Держит цель Multi-Aim Constraint головы Александра на камере игрока, чтобы он следил за Бет взглядом.
/// Висит на корне префаба Alexander_New. Без игрока в сцене (катсцены, тестовые сцены) цель стоит на месте.
/// </summary>
public class AlexanderHeadLook : MonoBehaviour
{
    /// <summary>Объект-источник Multi-Aim Constraint головы; его двигает этот скрипт.</summary>
    [SerializeField] private Transform _lookTarget;

    // Optional: префаб стоит и в катсценах, где игрок может быть не забинжен.
    [InjectOptional] private FirstPersonController _player;

    private Transform _overrideTarget;

    /// <summary>Смотреть на указанный объект вместо игрока, например на камеру катсцены поимки.</summary>
    public void LookAt(Transform target)
    {
        _overrideTarget = target;
    }

    private void Update()
    {
        if (_lookTarget == null)
        {
            return;
        }

        Transform target = _overrideTarget != null ? _overrideTarget : _player != null ? _player.View : null;
        if (target != null)
        {
            _lookTarget.position = target.position;
        }
    }
}
