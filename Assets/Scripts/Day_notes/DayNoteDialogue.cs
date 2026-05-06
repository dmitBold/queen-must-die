using Dialogue;
using System;
using UnityEngine;

public class DayNoteDialogue : MonoBehaviour
{
    [SerializeField] DialogueView view;

    public void Show(string[] pages)
    {
        view.Show();
        view.PlayText(pages);
        view.SetSkipVisible(true);
        view.SetBackVisible(view.typewriter.CanGoBack);
    }

    public void OnSkipPressed()
    {
        var result = view.Skip();

        view.SetBackVisible(view.typewriter.CanGoBack);

        if (view.IsFinished)
        {
            view.SetSkipVisible(false);
        }

        return;
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
    }

    void HandleTypewriterFinished()
    {
        view.SetSkipVisible(false);
    }

    void HandleTypewriterBack()
    {
        view.SetSkipVisible(true);
    }
}