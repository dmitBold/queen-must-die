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
        public Quest quest;
        public DayNoteData rewardNote;
        public int rewardAmount = 1;

        [Tooltip("Если задан, выбор недоступен без этого предмета в инвентаре")]
        public ItemData requiredItem;
        [Tooltip("Забрать требуемый предмет из инвентаря при выборе")]
        public bool consumeRequiredItem = true;

        [Tooltip("Если включено, факт выдачи наград этого выбора сохраняется (флаг в WorldState) и они выдаются один раз за всю игру. " +
                 "Если выключено, награды выдаются при каждом выборе, как раньше. Требуемый предмет списывается в любом случае.")]
        public bool saveGrantStatus = false;
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

        [Tooltip("Награды за страницы. Индекс элемента равен индексу страницы в CardPages. Может быть короче CardPages.")]
        [SerializeField] private CardPageReward[] pageRewards;
        public string Card_name;
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

        /// <summary>
        /// Награда за страницу. Возвращает false, если массив не задан, индекс вне границ, элемент null или награда пустая.
        /// </summary>
        public bool TryGetPageReward(int pageIndex, out CardPageReward reward)
        {
            reward = null;
            if (pageRewards == null || pageIndex < 0 || pageIndex >= pageRewards.Length) return false;

            var candidate = pageRewards[pageIndex];
            if (candidate == null || candidate.IsEmpty) return false;

            reward = candidate;
            return true;
        }

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