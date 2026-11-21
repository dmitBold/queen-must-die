using Core;
using FMODUnity;
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
        public EventReference interact_sound;
        public EventReference typeSound;
        bool play_once = true;

        public List<EventReference> dialogue_sounds;
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

            NightDialogueManager.Instance.StartDialogue(this, dialoguePages, dialogueNames, typeSound, () =>
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
            if (!interact_sound.IsNull)
            {
                if (play_once)
                {
                    Debug.Log("sss");
                   _audioService.PlayFMODEvent(interact_sound);
                    play_once = false;
                }
            }
            //TEST
        }
    }
}