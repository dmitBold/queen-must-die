using Core;
using Dialogue;
using Inventory;
using UnityEngine;
using Zenject;

namespace Cards
{
    public class CardManager : MonoBehaviour
    {
        public WorldState worldState;
        private CardData currData;
        //private bool wait_for_choice;
        //test
        public bool wait_for_choice;
        //test

        public System.Action OnCardResolved;
        public DialogueController dialogue;

        //test
        private InventoryManager inventory;

        [Inject]
        public void Constructor(InventoryManager inventoryManager)
        {
            this.inventory = inventoryManager;
        }
        public System.Action OnAnyChoiceResolved;


        //test
        public enum ChoiceAvailability
        {
            Available,
            LockedStat,
            MissingFlag,
            CustomBlocked
        }

        public void ShowCard(CardData card)
        {
            currData = card;
            wait_for_choice = true;
            Debug.Log(card.CardText);
            Debug.Log(card.LeftChoice.text + " | " +  card.RightChoice.text);
        }

        void ApplyChoice(Choice choice)
        {
            foreach (var effect in choice.effects)
            {
                switch (effect.type)
                {
                    case CardEffectType.LockStat:
                        //worldState.ApplyToStat(-100, effect.stat);
                        worldState.LockStat(effect.stat);
                        break;

                    case CardEffectType.ModifyStat:
                        worldState.ApplyToStat(effect.value, effect.stat);
                        break;
                }
            }

            foreach (var flag in choice.addFlags)
            {
                worldState.AddFlag(flag);
            }

            foreach (var flag in choice.removeFlags)
            {
                worldState.RemoveFlag(flag);
            }
        }

        /*public void ResolveChoice(Choice choice) {

        if (!wait_for_choice) return;

        wait_for_choice = false;
        ApplyChoice(choice);
        OnCardResolved?.Invoke();



    }*/

        public void ResolveChoice(Choice choice)
        {
            if (!wait_for_choice) return;

            //test  test    test

            //if(CanChoose(choice) != ChoiceAvailability.Available)
            //{
            //return;
            //}

            //test


            wait_for_choice = false;
            ApplyChoice(choice);

            //test
            if (choice.rewardItem != null)
            {
                inventory.AddItem(choice.rewardItem, choice.rewardAmount);
            }

            if (!string.IsNullOrEmpty(choice.reactionText))
            {
                dialogue.ShowReaction(choice.reactionText, EndCard);
            }
            else
            {
                EndCard();
            }

            //test
            OnAnyChoiceResolved?.Invoke();
        }

        void EndCard()
        {
            OnCardResolved?.Invoke();
        }
        //

        void Start()
        {
        
        }

        /*void Update()
    {
        if (!wait_for_choice) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ResolveChoice(currData.LeftChoice);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ResolveChoice(currData.RightChoice);
        }
    }*/

        public bool CanApplyItem(ItemData item)
        {
            if (!wait_for_choice) return false;
            if (currData == null) return false;

            return currData.GetReactionForItem(item) != null;
        }

        /*public void ApplyItem(ItemData item)
    {
        if (!wait_for_choice) return;

        ItemReaction reaction = currData.GetReactionForItem(item);
        if (reaction == null) return;

        wait_for_choice = false;

        ApplyItemReaction(reaction);

        if (!string.IsNullOrEmpty(reaction.reactionText))
        {
            dialogue.ShowReaction(reaction.reactionText, EndCard);
        }
        else
        {
            EndCard();
        }
    }*/

        public bool ApplyItem(ItemData item)
        {
            if (!wait_for_choice) return false;

            ItemReaction reaction = currData.GetReactionForItem(item);
            if (reaction == null) return false;

            wait_for_choice = false;

            ApplyItemReaction(reaction);

            if (!string.IsNullOrEmpty(reaction.reactionText))
            {
                dialogue.ShowReaction(reaction.reactionText, EndCard);
            }
            else
            {
                EndCard();
            }
            OnAnyChoiceResolved?.Invoke();
            return true;
        }

        void ApplyItemReaction(ItemReaction reaction)
        {
            foreach (var effect in reaction.effects)
            {
                switch (effect.type)
                {
                    case CardEffectType.LockStat:
                        worldState.LockStat(effect.stat);
                        break;

                    case CardEffectType.ModifyStat:
                        worldState.ApplyToStat(effect.value, effect.stat);
                        break;
                }
            }

            foreach (var flag in reaction.addFlags)
            {
                worldState.AddFlag(flag);
            }

            foreach (var flag in reaction.removeFlags)
            {
                worldState.RemoveFlag(flag);
            }
        }

        //test
        public void SkipCard()
        {
            if (!wait_for_choice)
                return;

            wait_for_choice = false;

            //TEST TEST TEST
            if (currData.BlockedFlags.Count > 0)
            {
                worldState.AddFlag(currData.BlockedFlags[0]);
            }
            //TEST TEST TEST

            worldState.ApplyEyePenalty();

            EndCard();
        }

        //test
        public ChoiceAvailability CanChoose(Choice choice)
        {
            foreach (var effect in choice.effects)
            {
                if (effect.type == CardEffectType.ModifyStat)
                {
                    if (effect.value < 0 && worldState.IsStatLocked(effect.stat))
                    {
                        return ChoiceAvailability.LockedStat;
                    }
                }
            }
            return ChoiceAvailability.Available;
        }


    }
}
