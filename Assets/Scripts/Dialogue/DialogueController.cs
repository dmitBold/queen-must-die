using System;
using UnityEngine;

namespace Dialogue
{
    public class DialogueController : MonoBehaviour
    {
        public event Action OnTextFinished;
        public event Action<int> OnPageFinished;

        [SerializeField] DialogueView view;

        private bool isNavigationLocked = false;

        enum Mode { Normal, Reaction }
        Mode mode;

        Action onReactionClosed;

        public void Show(string[] pages, string card_name)
        {
            isNavigationLocked = false;
            mode = Mode.Normal;
            view.Show();
            view.set_name(card_name);
            view.PlayText(pages);
            view.SetSkipVisible(true);
            view.SetBackVisible(view.typewriter.CanGoBack);
        }

        public void ShowReaction(string[] text, Action onClosed)
        {
            isNavigationLocked = false;
            mode = Mode.Reaction;
            onReactionClosed = onClosed;
            view.Show();

            view.PlayText(text);

            view.SetSkipVisible(true);
            view.SetBackVisible(false);
        }

        public void OnSkipPressed()
        {
            var result = view.Skip();

            if (!isNavigationLocked)
            {
                view.SetBackVisible(view.typewriter.CanGoBack);
            }
            if (mode == Mode.Normal) return;

            if (mode == Mode.Reaction && result == TypewriterEffect.SkipResult.DialogueFinished)
            {
                view.Hide();
                onReactionClosed?.Invoke();
            }
        }

        public void OnBackPressed()
        {
            view.typewriter.back();
            view.SetBackVisible(view.typewriter.CanGoBack);
        }

        public void Hide()
        {
            view.Hide();
        }

        public void Start()
        {
            view.Hide();
            view.typewriter.OnDialogueFinished += HandleTypewriterFinished;
            view.typewriter.OnDialogueBack += HandleTypewriterBack;
            view.typewriter.OnPageFinished += HandlePageFinished;
        }

        void HandlePageFinished(int page)
        {
            if (mode == Mode.Normal)
                OnPageFinished?.Invoke(page);
        }

        public void SetNavigationInteractable(bool interactable)
        {
            isNavigationLocked = !interactable;
            if (interactable)
            {
                view.SetSkipVisible(true);
                view.SetBackVisible(view.typewriter.CanGoBack);
            }
            else
            {
                view.SetSkipVisible(false);
                view.SetBackVisible(false);
            }
        }

        void HandleTypewriterFinished()
        {
            if (mode != Mode.Normal)
                return;

            view.SetSkipVisible(false);
            OnTextFinished?.Invoke();
        }

        void HandleTypewriterBack()
        {
            view.SetSkipVisible(true);
        }

        //TEST!!!!
        void Update()
        {
            HandleClick();
        }

        private void HandleClick()
        {
            if (Input.GetMouseButtonDown(0) && view.canSkip())
            {
                OnSkipPressed();
            }
        }
    }
}