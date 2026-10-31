using System.Collections;
using Core;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class StartDialogueSound : MonoBehaviour
    {
        public AudioClip StartSound;

        [Header(" ")]
        [TextArea(3, 5)]
        public string[] dialoguePages;
        public float delay;

        public PlayerStateController _playerStateController;


        private AudioService _audioService;
        private NightDialogueManager _dialogueManager;

        [Inject]
        public void Constructor(AudioService audioService, PlayerStateController playerStateController, NightDialogueManager dialogueManager)
        {
            _audioService = audioService;
            _playerStateController = playerStateController;
            _dialogueManager = dialogueManager;
        }

        void Start()
        {
            if (StartSound != null)
            {
                _audioService.PlaySound(StartSound);
                StartCoroutine(WaitAndExecute(delay));
            }
            //NightDialogueManager.Instance.StartDialogue(null, dialoguePages);

            //PlayerStateController.Instance.SetMode(PlayerStateController.PlayerMode.Focused);
        }

        IEnumerator WaitAndExecute(float duration)
        {
            yield return new WaitForSeconds(duration);
            OnSoundEnded();
        }

        void OnSoundEnded()
        {
            _dialogueManager.StartDialogue(null, dialoguePages);

            _playerStateController.SetMode(PlayerMode.Focused);
        }

        public void StartDialogue()
        {
            _dialogueManager.StartDialogue(null, dialoguePages);

            _playerStateController.SetMode(PlayerMode.Focused);
        }
    }
}
