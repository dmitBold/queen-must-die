using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using Zenject;

namespace NightCycle
{
    public class NightDialogueManager : MonoBehaviour
    {
        public static NightDialogueManager Instance;
        public event  Action DialogEnded;
        
        [Header("UI ������")] [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private NightTypewriter typewriter;

        private string[] currentPages;
        private int currentPageIndex;
        private NightDialogueInteractable currentNPC;
        private PlayerInteraction playerInteraction;

        private PlayableDirector currentDirector;

        //test
        public CinemachineBrain cameraBrain;
        //test

        [Inject]
        private void Construct([InjectOptional] PlayerInteraction playerInteraction)
        {
            this.playerInteraction = playerInteraction;
        }

        void Awake()
        {
            dialoguePanel.SetActive(false);
            Instance = this;
            
            HUDController.instance.SetCrosshairActivity(true);
        }

        void Update()
        {
            if (dialoguePanel.activeSelf && Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
        }

        public void StartDialogue(NightDialogueInteractable npc, string[] pages)
        {
            if (pages == null || pages.Length == 0) return;

            currentNPC = npc;
            currentPages = pages;
            currentPageIndex = 0;

            currentDirector = null;

            dialoguePanel.SetActive(true);
            PlayCurrentPage();
        }

        private void HandleClick()
        {
            if (typewriter.IsTyping)
            {
                typewriter.SkipTyping();
            }
            else
            {
                currentPageIndex++;

                if (currentPageIndex < currentPages.Length)
                {
                    PlayCurrentPage();
                }
                else
                {
                    EndDialogue();
                    //playerInteraction.ForceExitFocus();
                }
            }
        }

        private void PlayCurrentPage()
        {
            typewriter.TypeText(currentPages[currentPageIndex]);
        }

        /*public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
    }*/

        /*public void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        if (currentDirector != null)
        {
            //currentDirector.Play();
            currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
            //TEST TEST
            if (cameraBrain != null)
            {
                cameraBrain.enabled = true;
            }
            //TEST TEST
            currentDirector = null;
        }

    }*/

        public void EndDialogue()
        {
            dialoguePanel.SetActive(false);

            if (currentDirector != null)
            {
                //playerInteraction.ForceExitFocus();
                if (cameraBrain != null)
                {
                    cameraBrain.enabled = true;
                }

                currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
                currentDirector = null;
            }
            else
            {
                if (playerInteraction != null)
                {
                    playerInteraction.ForceExitFocus();
                }

                currentNPC = null;
            }

            DialogEnded?.Invoke();
        }

        public void ForceEndDialogue()
        {
            dialoguePanel.SetActive(false);
        }


        public void StartTimelineDialogue(string[] pages, PlayableDirector director, bool pauseTimeline)
        {
            if (pages == null || pages.Length == 0) return;

            currentNPC = null;
            currentPages = pages;
            currentPageIndex = 0;

            if (pauseTimeline && director != null)
            {
                currentDirector = director;
                //currentDirector.Pause();
                currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
                //TEST TEST
                if (cameraBrain != null)
                {
                    cameraBrain.enabled = false;
                }
                //TEST TEST
            }

            dialoguePanel.SetActive(true);
            PlayCurrentPage();
        }
    }
}