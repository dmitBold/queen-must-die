using UnityEngine;

public class NoteInteractable : MonoBehaviour, IFocusable
{
    public Sprite noteImage;

    [TextArea(5, 20)]
    public string noteText; // Текст записки

    public void OnEnterFocus()
    {
        NoteController.Instance.ShowNote(this);
    }

    public void OnExitFocus()
    {
        NoteController.Instance.CloseNote();
    }
}