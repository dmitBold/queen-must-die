using Cards;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core
{
    public class CrisisManager : MonoBehaviour
    {
        [SerializeField] DeckManager deck;
        [SerializeField] WorldState world;

        [Inject]
        public void Constructor(WorldState state)
        {
            this.world = state;
        }

        HashSet<WorldState.Stats> triggered = new();

        void Start()
        {
            world.OnStatBecameCritical += OnCrisis;
        }

        void OnCrisis(WorldState.Stats stat)
        {
            if (triggered.Contains(stat))
                return;

            triggered.Add(stat);

            TriggerCrisis(stat);
        }

        void TriggerCrisis(WorldState.Stats stat)
        {
            Debug.Log($"CRISIS STARTED: {stat}");

            deck.UnlockCrisisBranch(stat);
        }
    }
}
