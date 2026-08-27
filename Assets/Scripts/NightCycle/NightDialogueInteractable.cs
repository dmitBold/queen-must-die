using Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace NightCycle
{
    [RequireComponent(typeof(Interactable))]
    public class NightDialogueInteractable : MonoBehaviour, IFocusable
    {
        [Header("��������� �������")]
        [TextArea(3, 5)]
        public string[] dialoguePages;
        public string[] dialogueNames;
        //TEST
        public AudioClip interact_sound;
        bool play_once = true;

        public List<AudioClip> dialogue_sounds;
        public int dialogue_sounds_index = 0;
        //TEST

        //test
        [Header("События после диалога")]
        public UnityEvent onDialogueCompleted;
        public bool triggerEventOnlyOnce = true;
        private bool hasTriggeredEvent = false;
        //test

        private AudioService _audioService;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        public void OnEnterFocus()
        {
            PlayIneractionSound();

            NightDialogueManager.Instance.StartDialogue(this, dialoguePages, dialogueNames, () =>
            {
                if (triggerEventOnlyOnce && hasTriggeredEvent) return;

                onDialogueCompleted?.Invoke();
                hasTriggeredEvent = true;
            });
        }

        public void OnExitFocus()
        {
            NightDialogueManager.Instance.ForceEndDialogue();
        }

        public void PlayIneractionSound()
        {
            Debug.Log("sss");
            //TEST
            if (interact_sound != null)
            {
                if (play_once)
                {
                    Debug.Log("sss");
                   _audioService.PlaySound(interact_sound);
                    play_once = false;
                }
            }
            //TEST
        }
    }
}