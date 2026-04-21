using Cards;
using Choices;
using Dialogue;
using Inventory;
using NightCycle;
using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    public class DocsController : MonoBehaviour
    {
        public PageController pageController;
        public PagesManager pagesManager;
        public DayDialogueManager dialogue;
        public PageChoiceUI choiceUI;
        public FeatherAnim featherAnim;

        public UnityEvent onPagesEnded;

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
            Debug.Log("Docs Started");
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
            Debug.Log("SHOWPPPP");
            pageController.ShowPage();
        }

        void OnPageArrived()
        {
            Debug.Log("STARTTTTTDDDDDD");
            if (state != PageState.PageComing) return;

            state = PageState.ShowingDialogue;
            Debug.Log("STARTTTTTDDDDDD");
            dialogue.StartDialogue(currentPage.dialoguePages);
        }

        void OnDialogueFinished()
        {
            Debug.Log("DIALFINNNNNNNNNNNNNNNNNNN");
            if (state != PageState.ShowingDialogue) return;

            state = PageState.WaitingForChoice;
            choiceUI.Show(currentPage.LeftChoice, currentPage.RightChoice);
        }

        void OnChoiceSelected(PageChoice choice)
        {
            if (state != PageState.WaitingForChoice) return;

            choiceUI.Hide();
            dialogue.Hide();

            state = PageState.FeatherSigning;
            featherAnim.PlaySign();
        }

        void OnSignFinished()
        {
            if (state != PageState.FeatherSigning) return;

            state = PageState.PageLeaving;
            pageController.EndPage();
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
    }
}