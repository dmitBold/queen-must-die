using NightCycle;
using UnityEngine;
using UnityEngine.Playables;
using Zenject;

/// <summary>
/// Двигает цель Multi-Aim Constraint головы Александра. Приоритет целей: точка взгляда Бет, пока на экране реплика
/// диалогового таймлайна Александра; цель из LookAt; цель для чтения; камера игрока.
/// Висит на корне префаба Alexander_New. Без игрока в сцене (катсцены, тестовые сцены) цель стоит на месте.
/// </summary>
public class AlexanderHeadLook : MonoBehaviour
{
    /// <summary>Объект-источник Multi-Aim Constraint головы; его двигает этот скрипт.</summary>
    [SerializeField] private Transform _lookTarget;

    /// <summary>Куда смотреть вне диалога вместо игрока, например на бумаги, которые читает Александр. Пусто — следит за игроком.</summary>
    [SerializeField] private Transform _readTarget;

    /// <summary>Таймлайн с репликами Александра: пока открыто окно его реплики, Александр смотрит на Бет поверх всех остальных целей.</summary>
    [SerializeField] private PlayableDirector _dialogueDirector;

    /// <summary>Откуда смотрит Бет во время реплик, обычно камера катсцены диалога. Пусто — камера игрока.</summary>
    [SerializeField] private Transform _dialogueView;

    /// <summary>За сколько секунд взгляд переходит на новую цель. 0 — мгновенно.</summary>
    [SerializeField, Min(0f)] private float _retargetDuration = 0.5f;

    // Optional: префаб стоит и в катсценах, где игрок может быть не забинжен.
    [InjectOptional] private FirstPersonController _player;

    private Transform _overrideTarget;
    private Transform _currentTarget;
    private Vector3 _blendFrom;
    private float _blendElapsed;

    /// <summary>Смотреть на указанный объект вместо игрока и цели для чтения, например на камеру катсцены поимки.</summary>
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

        Transform target = ResolveTarget();
        if (target == null)
        {
            return;
        }

        if (target != _currentTarget)
        {
            // Multi-Aim применяет цель мгновенно, поэтому при смене цели сами ведём её от старой точки к новой.
            // Самую первую цель берём сразу, чтобы голова не ехала от позиции цели в сцене.
            _blendFrom = _lookTarget.position;
            _blendElapsed = _currentTarget == null ? _retargetDuration : 0f;
            _currentTarget = target;
        }

        _blendElapsed += Time.deltaTime;
        float t = _retargetDuration > 0f ? Mathf.Clamp01(_blendElapsed / _retargetDuration) : 1f;
        _lookTarget.position = Vector3.Lerp(_blendFrom, target.position, Mathf.SmoothStep(0f, 1f, t));
    }

    private Transform ResolveTarget()
    {
        Transform playerView = _player != null ? _player.View : null;

        // Пока камера катсцены выключена (первые реплики идут от лица игрока), Бет смотрит своей камерой.
        Transform dialogueView = _dialogueView != null && _dialogueView.gameObject.activeInHierarchy ? _dialogueView : playerView;
        if (dialogueView != null && IsTalking())
        {
            return dialogueView;
        }

        if (_overrideTarget != null)
        {
            return _overrideTarget;
        }

        return _readTarget != null ? _readTarget : playerView;
    }

    private bool IsTalking()
    {
        return _dialogueDirector != null
            && NightDialogueManager.Instance != null
            && NightDialogueManager.Instance.IsShowingTimelineDialogue(_dialogueDirector);
    }
}
