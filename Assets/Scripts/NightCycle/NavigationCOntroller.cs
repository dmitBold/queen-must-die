using NightCycle;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Zenject;
using Zenject.SpaceFighter;

public class NavigationCOntroller : MonoBehaviour
{
    public Transform position;

    public NavMeshAgent agent;

    [Inject] Player _player;

    public void StartMove()
    {
        agent.SetDestination(position.position);
    }

    private void Update()
    {
        if (_player != null && !_player.IsDead && agent.isOnNavMesh)
        {
            agent.SetDestination(_player.Position);
        }
    }



}
