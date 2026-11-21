using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Подгоняет темп шага Александра под фактическую скорость NavMeshAgent, чтобы ноги не скользили:
/// в подвале агент быстрее, чем наверху, а клип ходьбы общий. Висит на корне префаба Alexander_New;
/// агент ищется у родителя, без него (катсцены) темп остаётся обычным.
/// </summary>
[RequireComponent(typeof(Animator))]
public class AlexanderWalkSpeed : MonoBehaviour
{
    private static readonly int WalkSpeedParam = Animator.StringToHash("WalkSpeed");

    // Ниже этой скорости агент считается стоящим: темп не трогаем, чтобы шаг не замирал на разгоне.
    private const float MinMoveSpeed = 0.05f;

    /// <summary>Скорость в м/с, на которой клип шага идёт без скольжения при темпе 1.</summary>
    [SerializeField, Min(0.1f)] private float _clipSpeed = 1.76f;

    private Animator _animator;
    private NavMeshAgent _agent;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponentInParent<NavMeshAgent>();
    }

    private void Update()
    {
        if (_agent == null || !_agent.enabled)
        {
            return;
        }

        float speed = _agent.velocity.magnitude;
        _animator.SetFloat(WalkSpeedParam, speed > MinMoveSpeed ? speed / _clipSpeed : 1f);
    }
}
