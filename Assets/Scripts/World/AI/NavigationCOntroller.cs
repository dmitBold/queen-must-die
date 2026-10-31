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
