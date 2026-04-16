using Core;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    [RequireComponent(typeof(Interactable))]
    public class NightDialogueInteractable : MonoBehaviour, IFocusable
    {
        [Header("��������� �������")]
        [TextArea(3, 5)]
        public string[] dialoguePages;

        //TEST
        public AudioClip interact_sound;
        bool play_once = true;
        //TEST

        private AudioService _audioService;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        public void OnEnterFocus()
        {
            PlayIneractionSound();
            NightDialogueManager.Instance.StartDialogue(this, dialoguePages);
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