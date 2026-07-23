using DI;
using Inventory;
using NightCycle;
using System.Collections;
using UnityEngine;
using Zenject;

public class DelayedTeleport : MonoBehaviour
{
    [SerializeField] private Transform point;
    [SerializeField] private float delay;

    [SerializeField] private PlayerStateBridge playerStateBridge;

    public void TeleportWDelay()
    {
        StartCoroutine(TP_routine());
    }

    IEnumerator TP_routine()
    {
        yield return new WaitForSeconds(delay);

        playerStateBridge.MovePlayer(point);
    }

}
