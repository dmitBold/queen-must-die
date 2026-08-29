using UnityEngine;
using UnityEngine.Events;

namespace NightCycle
{
    public class NoteInteractable : MonoBehaviour, IFocusable
    {
        public Sprite noteImage;

        [TextArea(5, 20)]
        public string[] noteText;

        public bool trigger_once = true;
        public UnityEvent noteEvent;

        public void SetEvent(bool trigger)
        {
            trigger_once = trigger;
        }

        public void OnEnterFocus()
        {
            NoteController.Instance.ShowNote(this);
        }

        public void OnExitFocus()
        {
            NoteController.Instance.CloseNote();
            TriggerNoteEvent();
        }

        public void TriggerNoteEvent()
        {
            if (trigger_once)
            {
                noteEvent?.Invoke();
                trigger_once = false;
            }
        }

    }
}