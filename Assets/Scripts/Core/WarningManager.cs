using System.Collections.Generic;
using Cards;
using UnityEngine;

namespace Core
{
    public class WarningManager : MonoBehaviour
    {
        [Header("Config")]
        public WarningConfig config;
        public WorldState worldState;

        // ������� ����
        private Queue<CardData> warningQueue = new Queue<CardData>();

        void Start()
        {
            worldState.OnStatBecameCritical += HandleCriticalStat;
        }

        void OnDestroy()
        {
            if (worldState != null)
                worldState.OnStatBecameCritical -= HandleCriticalStat;
        }

        void HandleCriticalStat(WorldState.Stats stat)
        {
            CardData card = config.GetCardForStat(stat);

            if (card != null)
            {
                Debug.Log($"Warning added for: {stat}");
                warningQueue.Enqueue(card);
            }
            else
            {
                Debug.LogWarning($"No warning card found for stat: {stat}");
            }
        }

        public bool HasPendingWarnings()
        {
            return warningQueue.Count > 0;
        }

        public CardData GetNextWarning()
        {
            if (warningQueue.Count > 0)
                return warningQueue.Dequeue();

            return null;
        }
    }
}