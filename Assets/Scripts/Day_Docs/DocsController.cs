using Cards;
using Choices;
using Dialogue;
using Inventory;
using NightCycle;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using Zenject;

namespace Core
{
    public class DocsController : MonoBehaviour
    {
        public PageController pageController;
        public PagesManager pagesManager;
        public DayDialogueManager dialogue;
        public FeatherAnim featherAnim;

        public UnityEvent onPagesEnded;

        private WorldState worldState;
        public ChoiceUI choiceUI;
        public CardManager cardManager;

        [Inject]
        public void Constructor(WorldState state)
        {
            this.worldState = state;
        }

        enum PageState
        {
            Idle,
            PageComing,
            ShowingDialogue,
            WaitingForChoice,
            FeatherSigning,
            PageLeaving,
            End
        }

        PageState state = PageState.Idle;
        PageData currentPage = null;

        void Start()
        {
            pageController.OnPageArrived += OnPageArrived;
            pageController.OnPageLeft += OnPageLeft;
            dialogue.OnDialogueEnded += OnDialogueFinished;
            choiceUI.OnChoiceSelected += OnChoiceSelected;
            cardManager.OnAnyChoiceResolved += FinishChoice;
            choiceUI.OnEyeSelected += OnEyeSelected;

            if (featherAnim != null)
                featherAnim.OnSignFinished += OnSignFinished;

            //StartDocs();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T)) EndDocs();
        }

        public void StartDocs()
        {
            dialogue.gameObject.SetActive(true);
            NextDoc();
        }

        void NextDoc()
        {
            currentPage = pagesManager.GetPage();

            if (currentPage == null)
            {
                EndDocs();
                return;
            }

            state = PageState.PageComing;
            pageController.ShowPage();
            cardManager.SetCurrData(currentPage.cardData);
        }

        void OnPageArrived()
        {
            if (state != PageState.PageComing) return;

            state = PageState.ShowingDialogue;
            dialogue.StartDialogue(currentPage.dialoguePages);
        }

        void OnDialogueFinished()
        {
            if (state != PageState.ShowingDialogue) return;

            cardManager.wait_for_choice = true;
            state = PageState.WaitingForChoice;

            var left = currentPage.cardData.LeftChoice;
            var right = currentPage.cardData.RightChoice;

            bool left_availible = cardManager.CanChoose(left) == CardManager.ChoiceAvailability.Available;
            bool right_availible = cardManager.CanChoose(right) == CardManager.ChoiceAvailability.Available;


            choiceUI.SetChoiceAvailability(left_availible, right_availible);
            //test


            choiceUI.Show(
                currentPage.cardData.LeftChoice,
                currentPage.cardData.RightChoice
            );

            //choiceUI.OnChoiceSelected += OnChoiceSelected;

            state = PageState.WaitingForChoice;
        }

        void OnChoiceSelected(Choice choice)
        {
            if (state != PageState.WaitingForChoice) return;

            choiceUI.Hide();
            dialogue.Hide();

            FinishChoice();
            cardManager.ResolveChoice(choice);

            state = PageState.FeatherSigning;
            featherAnim.PlaySign();
        }

        void OnSignFinished()
        {
            if (state != PageState.FeatherSigning) return;

            state = PageState.PageLeaving;
            pageController.EndPage();
        }

        void FinishChoice()
        {
            //choiceUI.OnChoiceSelected -= OnChoiceSelected;
            choiceUI.Hide();
        }

        void OnPageLeft()
        {
            if (state != PageState.PageLeaving) return;
            NextDoc();
        }

        void EndDocs()
        {
            state = PageState.End;
            onPagesEnded.Invoke();
        }

        void OnEyeSelected()
        {
            if (state != PageState.WaitingForChoice)
                return;

            choiceUI.Hide();

            //inventoryUI.CancelDrag();

            //cardManager.SkipCard();
            cardManager.SkipCard();
            dialogue.Hide();
            FinishChoice();
            state = PageState.PageLeaving;
            pageController.EndPage();
            /*worldState.ApplyEyePenalty();
            FinishChoice();
            cardManager.OnAnyChoiceResolved?.Invoke();
            state = PageState.PageLeaving;
            pageController.EndPage();*/
        }
    }
}