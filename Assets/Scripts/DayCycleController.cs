using System.Collections.Generic;
using UnityEngine;
using static CardManager;

public class DayCycleController : MonoBehaviour
{

    public WorldState worldState;
    public CardManager cardManager;
    public DeckManager deckManager;
    public VisitorController visitor;
    public DialogueController dialogue;
    public ChoiceUI choiceUI;
    public InventoryManager inventory;
    public InventoryUI inventoryUI;

    //test
    public DayPlan dayPlan;
    public LevelLoader LevelLoader;
    public WarningManager warningManager;
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
        //if (state != DayState.ShowingDialogue) return;


        //cardManager.ShowCard(visitor.currentCard);
        //deckManager.NotifyCardShown(visitor.currentCard);

        //state = DayState.WaitingForChoice;
        state = DayState.ShowingDialogue;

        dialogue.Show(visitor.currentCard.CardText);

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


    void EndDay()
    {
        //CurrentState = DayState.EndOfDay;
        Debug.Log("Day ended. Total visitors: " + currentSlotIndex);
        LevelLoader.LoadNext();
    }

    void Start()
    {
        visitor.OnVisitorArrived += OnVisitorArrived;
        visitor.OnVisitorLeft += OnVisitorLeft;
        cardManager.OnCardResolved += OnCardResolved;

        dialogue.OnTextFinished += OnDialogueFinished;

        //test
        choiceUI.OnEyeSelected += OnEyeSelected;

        //
        inventoryUI.Init(inventory, this);

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

        choiceUI.Hide();

        inventoryUI.CancelDrag();

        cardManager.SkipCard();
    }

}
