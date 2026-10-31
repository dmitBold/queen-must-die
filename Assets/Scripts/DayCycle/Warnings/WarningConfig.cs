using System.Collections.Generic;
using Cards;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "WarningConfig", menuName = "Game/Warning Config")]
    public class WarningConfig : ScriptableObject
    {
        [System.Serializable]
        public struct WarningEntry
        {
            public WorldState.Stats stat;
            public CardData warningCard;
        }

        public List<WarningEntry> warnings;

        public CardData GetCardForStat(WorldState.Stats stat)
        {
            foreach (var entry in warnings)
            {
                if (entry.stat == stat) return entry.warningCard;
            }
            return null;
        }
    }
}