using Core;
using Inventory;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Cards
{
    public class DeckManager : MonoBehaviour
    {

        public List<CardData> AllCards;
        private WorldState worldState;

        public Dictionary<CardData, int> last_seen;
        public int currentTurn = 0;

        [Inject]
        public void Constructor(WorldState state)
        {
            this.worldState = state;
        }

        bool CheckCard(CardData card)
        {
            if(worldState.WeeksPassed < card.MinWeek) { return false; }
            if(worldState.WeeksPassed > card.MaxWeek) { return false; }

            foreach (var flag in card.RequiredFlags)
            {
                if (!worldState.flags.Contains(flag))
                {
                    return false;
                }
            }

            foreach (var flag in card.BlockedFlags)
            {
                if (worldState.flags.Contains(flag))
                {
                    return false;
                }
            }

            return true;
        }

        public void NotifyCardShown(CardData card)
        {
            currentTurn++;
            last_seen[card] = currentTurn;
        }
        /*public CardData GetRandomCard()
    {
        List<CardData> cards = GetAvailibleCards();

        //cards.RemoveAll(card => card.cooldown > (currentTurn - last_seen[card]));
        List<CardData> cards_filtered = cards.FindAll(card => card.cooldown == 0 || !last_seen.ContainsKey(card) 
        || currentTurn - last_seen[card] >= card.cooldown);



        int TotalWeight = 0;

        foreach (var card_s in cards_filtered)
        {
            if (card_s.weight > 0) { TotalWeight += card_s.weight; }
        }

        if (TotalWeight == 0 || cards_filtered.Count == 0)
        {
            return null;
        }

        int roll = Random.Range(0, TotalWeight);

        CardData target_card;
        int curr = 0;
        foreach (var card_t in cards_filtered)
        {
            curr += card_t.weight;
            if(roll < curr)
            {
                target_card = card_t;
                return target_card;
            }
        }

        return null;

    }*/

        public CardData GetMandatoryCard()
        {
            List<CardData> cards = GetAvailibleCards();

            //cards.RemoveAll(card => card.cooldown > (currentTurn - last_seen[card]));
            List<CardData> cards_filtered = cards.FindAll(card => (card.cooldown == 0 || !last_seen.ContainsKey(card)
                                                                                      || currentTurn - last_seen[card] >= card.cooldown) && (card.IsMandatory == true));



            int TotalWeight = 0;

            foreach (var card_s in cards_filtered)
            {
                if (card_s.weight > 0) { TotalWeight += card_s.weight; }
            }

            if (TotalWeight == 0 || cards_filtered.Count == 0)
            {
                return null;
            }

            int roll = Random.Range(0, TotalWeight);

            CardData target_card;
            int curr = 0;
            foreach (var card_t in cards_filtered)
            {
                curr += card_t.weight;
                if (roll < curr)
                {
                    target_card = card_t;
                    return target_card;
                }
            }

            return null;
        }

        public CardData GetRandomCard()
        {
            List<CardData> cards = GetAvailibleCards();

            //cards.RemoveAll(card => card.cooldown > (currentTurn - last_seen[card]));
            List<CardData> cards_filtered = cards.FindAll(card => (card.cooldown == 0 || !last_seen.ContainsKey(card)
                                                                                      || currentTurn - last_seen[card] >= card.cooldown) && (card.IsRandom == true));



            int TotalWeight = 0;

            foreach (var card_s in cards_filtered)
            {
                if (card_s.weight > 0) { TotalWeight += card_s.weight; }
            }

            if (TotalWeight == 0 || cards_filtered.Count == 0)
            {
                return null;
            }

            int roll = Random.Range(0, TotalWeight);

            CardData target_card;
            int curr = 0;
            foreach (var card_t in cards_filtered)
            {
                curr += card_t.weight;
                if (roll < curr)
                {
                    target_card = card_t;
                    return target_card;
                }
            }

            return null;
        }

        public List<CardData> GetAvailibleCards()
        {
            List<CardData> result = new();

            foreach (var card in AllCards)
            {
                if (!CheckCard(card)) continue;
                result.Add(card);
            }

            return result;
        }

        public void UnlockCrisisBranch(WorldState.Stats stat)
        {
            string flag = stat switch
            {
                WorldState.Stats.WealthStat => "CRISIS_WEALTH",
                WorldState.Stats.SnakeStat => "CRISIS_SNAKES",
                WorldState.Stats.SpiderStat => "CRISIS_SPIDERS",
                WorldState.Stats.WaspStat => "CRISIS_WASPS",
                WorldState.Stats.OrderStat => "CRISIS_ORDER",
                WorldState.Stats.PeopleStat => "CRISIS_PEOPLE",
                _ => ""
            };

            if (!string.IsNullOrEmpty(flag))
                worldState.AddFlag(flag);
        }


        public void Start()
        {
            last_seen = new Dictionary<CardData, int>();
        }

    }
}
