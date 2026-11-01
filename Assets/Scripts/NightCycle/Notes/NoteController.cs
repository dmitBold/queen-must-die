using Dialogue;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class NoteController : MonoBehaviour
    {
        public static NoteController Instance;

        [SerializeField] NoteView view;

        private bool isActive = false;
        [SerializeField] PlayerInteraction playerInteraction;

        private NoteInteractable current_note = null;

        [Inject]
        private void Construct(PlayerInteraction playerInteraction)
        {
            this.playerInteraction = playerInteraction;
        }
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            view.Hide();
        }

        public void ShowNote(NoteInteractable note)
        {
            current_note = note;

            if (isActive) return;

            isActive = true;

            view.Show(note.noteImage, note.noteText);


            HUDController.instance.DisableInteractionText();
        }

        public void CloseNote()
        {
            if (current_note != null)
            {
                current_note.TriggerNoteEvent();
            }

            if (!isActive) return;

            isActive = false;
            view.Hide();
        }

        private void Update()
        {
            // Esc обрабатывает UIManager и вызывает выход из фокуса через PlayerInteraction.
            if (!isActive || !view.Tintroot.activeSelf) return;

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetMouseButtonDown(0))
            {
                OnNextPressed();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetMouseButtonDown(1))
            {
                OnBackPressed();
            }
        }

        public void OnNextPressed()
        {
            var result = view.Skip();

            if (result == TypewriterEffect.SkipResult.DialogueFinished)
            {

                if (playerInteraction != null)
                {
                    playerInteraction.ForceExitFocus();
                }
                else
                {
                    CloseNote();
                }
            }
        }

        public void OnBackPressed()
        {
            view.Back();
        }
    }
}