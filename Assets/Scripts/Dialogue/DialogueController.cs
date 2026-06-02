using System;
using UnityEngine;

namespace Dialogue
{
    public class DialogueController : MonoBehaviour
    {
        public event Action OnTextFinished;

        [SerializeField] DialogueView view;

        enum Mode { Normal, Reaction }
        Mode mode;

        Action onReactionClosed;

        public void Show(string text)
        {
            mode = Mode.Normal;
            view.Show();
            view.PlayText(text);
            view.SetSkipVisible(true);
            view.SetBackVisible(view.typewriter.CanGoBack);
        }

        public void ShowReaction(string text, Action onClosed)
        {
            mode = Mode.Reaction;
            onReactionClosed = onClosed;
            view.Show();
            view.PlayText(text);
            view.SetSkipVisible(true);
        }

        /*public void OnSkipPressed()
    {
        var result = view.Skip();

        if (mode == Mode.Normal)
        {
            if (result == TypewriterEffect.SkipResult.DialogueFinished)
            {
                view.SetSkipVisible(false);
                OnTextFinished?.Invoke();
            }
        }
        else
        {
            if (result == TypewriterEffect.SkipResult.DialogueFinished)
            {
                view.Hide();
                onReactionClosed?.Invoke();
            }
        }
    }*/

        public void OnSkipPressed()
        {
            var result = view.Skip();

            view.SetBackVisible(view.typewriter.CanGoBack);

            if (mode == Mode.Normal)
            {
                if (view.IsFinished)
                {
                    view.SetSkipVisible(false);
                    OnTextFinished?.Invoke();
                }

                return;
            }

            if (mode == Mode.Reaction && result == TypewriterEffect.SkipResult.DialogueFinished)
            {
                view.Hide();
                onReactionClosed?.Invoke();
            }
        }

        //test
        public void OnBackPressed()
        {
            view.typewriter.back();
            view.SetBackVisible(view.typewriter.CanGoBack);
        }
        //test


        public void Hide()
        {
            view.Hide();
        }

        public void Start()
        {
            view.Hide();
            //
            view.typewriter.OnDialogueFinished += HandleTypewriterFinished;

            //test
            view.typewriter.OnDialogueBack += HandleTypewriterBack;
            //test
        }

        //
        void HandleTypewriterFinished()
        {
            if (mode != Mode.Normal)
                return;

            view.SetSkipVisible(false);
            OnTextFinished?.Invoke();
        }

        //test
        void HandleTypewriterBack()
        {
            view.SetSkipVisible(true);
        }
        //test

    }
}
