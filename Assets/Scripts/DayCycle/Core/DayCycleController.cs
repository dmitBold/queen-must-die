using Cards;
using Choices;
using Dialogue;
using Inventory;
using NightCycle;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Visitors;
using Zenject;
using static Cards.CardManager;

namespace Core
{
    public class DayCycleController : MonoBehaviour
    {

        private WorldState worldState;
        public CardManager cardManager;
        public DeckManager deckManager;
        public VisitorController visitor;
        public DialogueController dialogue;
        public ChoiceUI choiceUI;
        public InventoryUI inventoryUI;

        //test
        public DayPlan dayPlan;
        public LevelLoader LevelLoader;
        public WarningManager warningManager;
        public UnityEvent onDayEnded;
        //test

        [Inject]
        public void Constructor(WorldState state)
        {
            this.worldState = state;
        }

        //test
        private HashSet<int> resolvedIntermediatePages = new HashSet<int>();
        // Страницы текущей карты, награда за которые уже выдана в этот показ (защита от повторов при листании назад-вперёд)
        private HashSet<int> grantedPageRewards = new HashSet<int>();
        private int currentIntermediatePage = -1;
        //test

        enum DayState
        {
            Idle,
            VisitorComing,
            ShowingCard,
            ShowingDialogue,
            WaitingForChoice,
            VisitorLeaving,
            EndOfDay
        }

        DayState state = DayState.Idle;

        //TEST TEST TEST TEST
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                EndDay();
            }
        }
        //TEST TEST TEST TEST

        //public int MaxVisitors = 9;
        //private int CurrentVisitor = 0;

        //private DayState CurrentState = DayState.Idle;

        //test
        public int currentSlotIndex = 0;
        //test

        void StartDay()
        {
            //CurrentVisitor = 0;
            currentSlotIndex = 0;
            Debug.Log("Day Started");
            NextVisitor();
        }

        void NextVisitor()
        {
            /*if (CurrentVisitor >= MaxVisitors)
        {
            EndDay();
            return;
        }

        CurrentVisitor++;
        state = DayState.VisitorComing;

        CardData card = deckManager.GetRandomCard();

        Debug.Log("Deck returned card: " + (card != null ? card.name : "NULL"));


        if (card == null)
        {
            Debug.LogError("NO CARD FROM DECK");
            EndDay();
            return;
        }

        

        visitor.Spawn(card);*/

            //TEST
            if (warningManager.HasPendingWarnings())
            {
                Debug.Log("Showing Warning Card interrupt");

                CardData warningCard = warningManager.GetNextWarning();


                visitor.Spawn(warningCard);

                state = DayState.VisitorComing;
                return;
            }
            //TEST

            if (currentSlotIndex >= dayPlan.slots.Count)
            {
                EndDay();
                return;
            }
            state = DayState.VisitorComing;

            DaySlot slot = dayPlan.slots[currentSlotIndex];
            CardData card = null;

            switch (slot.type)
            {
                case DaySlotType.Mandatory:
                    card = deckManager.GetMandatoryCard();
                    break;

                case DaySlotType.Random:
                    card = deckManager.GetRandomCard();
                    break;
            }

            currentSlotIndex++;

            if (card == null)
            {
                Debug.LogError("NO CARD FOR SLOT: " + slot.type);
                EndDay();
                return;
            }

            Debug.Log($"Slot {currentSlotIndex}: {slot.type} → {card.name}");
            visitor.Spawn(card);

        }

        void OnVisitorArrived()
        {
            if (state != DayState.VisitorComing) return;

            resolvedIntermediatePages.Clear();
            grantedPageRewards.Clear();
            //
            currentIntermediatePage = -1;
            //
            state = DayState.ShowingDialogue;
            
            dialogue.Show(visitor.currentCard.CardPages, visitor.currentCard.Card_name); 
        }

        void GrantPageRewardOnce(int pageIndex)
        {
            if (visitor.currentCard == null) return;
            if (!grantedPageRewards.Add(pageIndex)) return;

            cardManager.GrantPageReward(visitor.currentCard, pageIndex);
        }

        void OnVisitorLeft()
        {
            if (state != DayState.VisitorLeaving) return;

            NextVisitor();
        }

        /*void OnCardResolved()
    {
        if (state != DayState.WaitingForChoice) return;

        state = DayState.VisitorLeaving;
        visitor.Despawn();
    }*/


        void OnCardResolved()
        {
            if (state != DayState.WaitingForChoice) return;
            dialogue.Hide();
            state = DayState.VisitorLeaving;
            visitor.Despawn();
        }

        void OnDialogueFinished()
        {
            if (state != DayState.ShowingDialogue) return;

            //dialogue.Hide();

            // Последняя страница приходит без индекса (событие окончания диалога), поэтому её награда выдаётся здесь,
            // до расчёта доступности финального выбора
            var cardPages = visitor.currentCard.CardPages;
            if (cardPages != null && cardPages.Length > 0)
            {
                GrantPageRewardOnce(cardPages.Length - 1);
            }

            cardManager.ShowCard(visitor.currentCard);

            deckManager.NotifyCardShown(visitor.currentCard);

            //test

            var left = visitor.currentCard.LeftChoice;
            var right = visitor.currentCard.RightChoice;

            bool left_availible = cardManager.CanChoose(left) == CardManager.ChoiceAvailability.Available;
            bool right_availible = cardManager.CanChoose(right) == CardManager.ChoiceAvailability.Available;


            choiceUI.SetChoiceAvailability(left_availible, right_availible);
            //test


            choiceUI.Show(
                visitor.currentCard.LeftChoice,
                visitor.currentCard.RightChoice
            );

            choiceUI.OnChoiceSelected += OnChoiceSelected;

            state = DayState.WaitingForChoice;
        }

        void OnChoiceSelected(Choice choice)
        {
            //test test test

            if (cardManager.CanChoose(choice) != ChoiceAvailability.Available)
            {
                return;
            }

            //test test test


            if (state != DayState.WaitingForChoice) return;

            //choiceUI.OnChoiceSelected -= OnChoiceSelected;
            //choiceUI.Hide();
            FinishChoice();

            cardManager.ResolveChoice(choice);
        }


        //void EndDay()
        //{
            //CurrentState = DayState.EndOfDay;
            //Debug.Log("Day ended. Total visitors: " + currentSlotIndex);
            //LevelLoader.LoadNext();
        //}

        void EndDay()
        {
            Debug.Log("Day ended. Total visitors: " + currentSlotIndex);
            //TEST
            //cardManager.SkipCard();
            dialogue.Hide();
            state = DayState.EndOfDay;
            //visitor.Despawn();
            FinishChoice();
            currentSlotIndex = dayPlan.slots.Count;
            //TEST
            onDayEnded.Invoke();
        }

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            HUDController.instance.DisableInteractionText();
            
            visitor.OnVisitorArrived += OnVisitorArrived;
            visitor.OnVisitorLeft += OnVisitorLeft;
            cardManager.OnCardResolved += OnCardResolved;

            dialogue.OnTextFinished += OnDialogueFinished;
            dialogue.OnPageFinished += HandlePageFinished;

            //test
            choiceUI.OnEyeSelected += OnEyeSelected;

            //
            //inventoryUI.Init(this);

            //test
            cardManager.OnAnyChoiceResolved += FinishChoice;

            StartDay();
        }

        //test
        public bool IsWaitingForChoice()
        {
            return state == DayState.WaitingForChoice;
        }


        //test
        void FinishChoice()
        {
            choiceUI.OnChoiceSelected -= OnChoiceSelected;
            choiceUI.Hide();
        }

        //test
        void OnEyeSelected()
        {
            if (state != DayState.WaitingForChoice)
                return;

            //SUPER TEST
            if (currentIntermediatePage != -1) return;
            //

            choiceUI.Hide();

            //inventoryUI.CancelDrag();

            cardManager.SkipCard();
        }

        void HandlePageFinished(int pageIndex)
        {
            if (state != DayState.ShowingDialogue) return;

            // Награда за страницу выдаётся до логики промежуточного выбора, чтобы предмет мог его разблокировать
            GrantPageRewardOnce(pageIndex);

            if (resolvedIntermediatePages.Contains(pageIndex)) return;
            var interChoice = visitor.currentCard.intermediateChoices?.Find(c => c.triggerAfterPage == pageIndex);

            if (interChoice != null)
            {
                state = DayState.WaitingForChoice;
                currentIntermediatePage = pageIndex;

                dialogue.SetNavigationInteractable(false);

                bool left_availible = cardManager.CanChoose(interChoice.leftChoice) == CardManager.ChoiceAvailability.Available;
                bool right_availible = cardManager.CanChoose(interChoice.rightChoice) == CardManager.ChoiceAvailability.Available;

                choiceUI.SetChoiceAvailability(left_availible, right_availible);

                choiceUI.Show(interChoice.leftChoice, interChoice.rightChoice);
                choiceUI.OnChoiceSelected += OnIntermediateChoiceSelected;
            }
        }

        void OnIntermediateChoiceSelected(Choice choice)
        {
            if (cardManager.CanChoose(choice) != ChoiceAvailability.Available) return;

            choiceUI.OnChoiceSelected -= OnIntermediateChoiceSelected;
            choiceUI.Hide();

            resolvedIntermediatePages.Add(currentIntermediatePage);
            //
            currentIntermediatePage = -1;
            //
            cardManager.ResolveIntermediateChoice(choice, visitor.currentCard);

            state = DayState.ShowingDialogue;
            dialogue.SetNavigationInteractable(true);
        }

    }
}
