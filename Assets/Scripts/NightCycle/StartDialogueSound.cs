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

        [Inject]
        public void Constructor(AudioService audioService, PlayerStateController playerStateController)
        {
            _audioService = audioService;
            _playerStateController = playerStateController;
        }

        void Start()
        {
            _audioService.PlaySound(StartSound);
            StartCoroutine(WaitAndExecute(delay));
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
            NightDialogueManager.Instance.StartDialogue(null, dialoguePages);

            _playerStateController.SetMode(PlayerMode.Focused);
        }
    }
}
