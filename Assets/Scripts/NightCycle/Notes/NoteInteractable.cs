using Core;
using FMODUnity;
using Inventory;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace NightCycle
{
    public class NoteInteractable : MonoBehaviour, IFocusable
    {
        public Sprite noteImage;

        [TextArea(5, 20)]
        public string[] noteText;

        public bool trigger_once = true;
        public UnityEvent noteEvent;
        public EventReference interact_sound;
        public bool trigger_sound_once = false;
        private bool soundFlag = true;

        public DayNoteData noteData;

        /// <summary>Хинт про журнал записок: один раз за прохождение, после закрытия первой записки, попавшей в журнал.</summary>
        [SerializeField] private string journalHintText = "Нажмите N, чтобы открыть журнал записок";
        [SerializeField] private float journalHintDuration = 4f;
        [SerializeField] private float journalHintFade = 0.5f;

        private const string JournalHintFlag = "hint:notes_journal";

        private AudioService _audioService;
        private DayNoteManager notemanager;
        private HintManager _hints;
        private WorldState _worldState;

        private bool noteAdded = false;


        [Inject]
        public void Constructor(AudioService audioService, DayNoteUI formanager, HintManager hintManager, WorldState worldState)
        {
            _audioService = audioService;
            notemanager = formanager.NoteManager;
            _hints = hintManager;
            _worldState = worldState;
        }

        public void SetEvent(bool trigger)
        {
            this.trigger_once = trigger;
        }

        public void OnEnterFocus()
        {
            if (soundFlag)
            {
                _audioService.PlayFMODEvent(interact_sound, transform.position);
                if (trigger_sound_once)
                {
                    soundFlag = false;
                }
            }
            NoteController.Instance.ShowNote(this);
            if (!noteAdded && noteData != null)
            {
                notemanager.AddNote(noteData);
                noteAdded = true;
            }
        }

        public void OnExitFocus()
        {
            NoteController.Instance.CloseNote();
            TriggerNoteEvent();
            // После закрытия, а не при открытии: пока записка открыта, окно журнала не откроется, а хинт перекрыт запиской.
            if (noteAdded) ShowJournalHint();
        }

        private void ShowJournalHint()
        {
            if (_hints == null || _worldState == null || _worldState.HasFlag(JournalHintFlag)) return;
            _worldState.AddFlag(JournalHintFlag);
            _hints.ShowHint(journalHintText, journalHintDuration, journalHintFade);
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