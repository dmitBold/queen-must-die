using System.Collections.Generic;
using Core;
using Inventory;
using UnityEngine;
using Visitors;

namespace Cards
{
    public enum CardEffectType
    {
        ModifyStat,
        LockStat
    }

    [System.Serializable]
    public class Effect
    {
        public CardEffectType type;
        public WorldState.Stats stat;
        public int value;
    }

    [System.Serializable]
    public class Choice
    {
        public string text;
        public List<Effect> effects;
        public List<string> addFlags;
        public List<string> removeFlags;

        //[TextArea]
        //public string reactionText;
        [TextArea(3, 5)]
        public string[] reactionText;

        //test
        public ItemData rewardItem;
        public DayNoteData rewardNote;
        public int rewardAmount = 1;
    }

//testtesttest
    [System.Serializable]
    public class ItemReaction
    {
        public ItemData item;

        public List<Effect> effects;
        public List<string> addFlags;
        public List<string> removeFlags;

        [TextArea(3, 5)]
        public string[] reactionText;
    }

    [System.Serializable]
    public class IntermediateChoice
    {
        public int triggerAfterPage;
        public Choice leftChoice;
        public Choice rightChoice;
    }


    [CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
    public class CardData : ScriptableObject
    {
        [TextArea(3, 5)]
        public string[] CardPages;
        public Choice LeftChoice;
        public Choice RightChoice;

        public List<string> RequiredFlags;
        public List<string> BlockedFlags;
        public int MinWeek;
        public int MaxWeek;
        public int weight;

        public int cooldown;

        public VisitorData visitor;

        //test
        public List<ItemReaction> itemReactions;
        public List<IntermediateChoice> intermediateChoices;

        //test

        //test
        public bool IsMandatory;
        public bool IsRandom;
        //test

        public ItemReaction GetReactionForItem(ItemData item)
        {
            foreach (var reaction in itemReactions)
            {
                if (reaction.item == item)
                    return reaction;
            }
            return null;
        }
    }
}