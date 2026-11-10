using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Zenject.SpaceFighter;

public class NavigationController : MonoBehaviour
{
    public NavMeshAgent agent;
    public float updateRate = 0.2f;

    private Coroutine routine;
    private Transform targetTransform;
    private Vector3 targetPosition;
    private bool isFollowingTransform;

    [Inject]
    private Player _player;

    private const string DoorLayerName = "DoorLayer";
    private const float DoorSearchRadius = 4f;
    private const float MinMoveSpeed = 0.1f;

    private readonly Collider[] _doorHits = new Collider[8];
    private int _doorMask;
    private Vector3 _lastPosition;

    private void Awake()
    {
        _doorMask = LayerMask.GetMask(DoorLayerName);
        _lastPosition = agent != null ? agent.transform.position : transform.position;
    }

    private void Update()
    {
        KickDoorsOnTheWay();
    }

    // Направление по фактическому смещению: на NavMeshLink скорость агента не отражает движение.
    private void KickDoorsOnTheWay()
    {
        if (agent == null || !agent.enabled)
        {
            return;
        }

        Vector3 position = agent.transform.position;
        Vector3 moveDirection = position - _lastPosition;
        _lastPosition = position;
        moveDirection.y = 0f;

        if (Time.deltaTime <= 0f || moveDirection.magnitude / Time.deltaTime < MinMoveSpeed)
        {
            moveDirection = agent.desiredVelocity;
            moveDirection.y = 0f;
            if (moveDirection.sqrMagnitude < MinMoveSpeed * MinMoveSpeed)
            {
                return;
            }
        }

        int hitCount = Physics.OverlapSphereNonAlloc(position, DoorSearchRadius, _doorHits, _doorMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hitCount; i++)
        {
            if (_doorHits[i].TryGetComponent(out NightCycle.HoldDoor door))
            {
                door.TryKick(position, moveDirection);
            }
        }
    }

    // Движение за движущейся целью (трансформом)
    public void StartMove(Transform target)
    {
        if (target == null) return;

        ResetMovement();
        targetTransform = target;
        isFollowingTransform = true;
        agent.isStopped = false;

        routine = StartCoroutine(UpdatePathRoutine());
    }

    // Движение за игроком
    public void MovePlayer()
    {
        if (_player != null && !_player.IsDead && agent.isOnNavMesh)
        {
            StartMove(_player.Rigidbody.transform); // Используем общий метод следования
        }
    }

    // Остановка агента
    public void StopMove()
    {
        ResetMovement();
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true; // Принудительно тормозим агента
            agent.ResetPath();      // Сбрасываем старый путь
        }
    }

    private void ResetMovement()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        isFollowingTransform = false;
        targetTransform = null;
    }

    IEnumerator UpdatePathRoutine()
    {
        while (true)
        {
            if (agent.isOnNavMesh)
            {
                // Если следим за объектом — берем его живую позицию
                // Иначе — агент идет в последнюю известную точку
                Vector3 currentTarget = isFollowingTransform ? targetTransform.position : targetPosition;
                agent.SetDestination(currentTarget);
            }
            yield return new WaitForSeconds(updateRate);
        }
    }
}
