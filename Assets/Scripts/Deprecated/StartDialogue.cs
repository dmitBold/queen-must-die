using Core;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using Zenject.SpaceFighter;

namespace NightCycle
{
    public class StartDialogue : MonoBehaviour
    {
        //public NightDialogueManager manager;

        [Header("Настройки текста")]
        //public string dialogueName;
        [TextArea(3, 5)]
        public string[] dialoguePages;
        public string[] dialogueNames; // Новый массив для кастомных имен страниц
        public float delay;

        [Header("События")]
        public UnityEvent onDialogueCompleted;
        public bool triggerEventOnlyOnce = true;
        private bool hasTriggeredEvent = false;

        public PlayerStateController _playerStateController;
        private AudioService _audioService;

        [Inject] Player _player;

        [Inject]
        public void Constructor(AudioService audioService, PlayerStateController playerStateController)
        {
            _audioService = audioService;
            _playerStateController = playerStateController;
        }

        public void startDialogue()
        {
            Debug.Log("SSDD");
            _playerStateController.SetMode(PlayerMode.Focused);

            NightDialogueManager.Instance.StartDialogue(null, dialoguePages, dialogueNames, () =>
            {


                if (triggerEventOnlyOnce && hasTriggeredEvent)
                {
                    Debug.Log("trigger dialogue end once");
                    return;
                }

                onDialogueCompleted?.Invoke();
                hasTriggeredEvent = true;
            });

            /*manager.StartDialogue(null, dialoguePages, () =>
            {


                if (triggerEventOnlyOnce && hasTriggeredEvent) return;

                onDialogueCompleted?.Invoke();
                hasTriggeredEvent = true;
            });*/
        }

        public void hidePLyaerModel()
        {
            _player.SetVisibility(false);
        }

        public void FreezeCharacter()
        {
            _playerStateController.SetMode(PlayerMode.Focused);
        }

        public void UnFreezeCharacter()
        {
            _playerStateController.SetMode(PlayerMode.FreeMovement);
        }
    }
}