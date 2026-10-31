using Core;
using System;
using TMPro;
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
        
        [Header("UI ������")]
        [SerializeField] private TextMeshProUGUI dialogueName;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private NightTypewriter typewriter;

        private string[] currentPages;
        private string[] currentNames;
        private string defaultName = " ";
        private int currentPageIndex;
        private NightDialogueInteractable currentNPC;
        private PlayerInteraction playerInteraction;

        private PlayableDirector currentDirector;
        // Director whose timeline dialogue is currently shown (paused or not).
        private PlayableDirector currentTimelineDirector;

        //test
        public CinemachineBrain cameraBrain;
        private AudioService _audioService;

        //Test
        private Action currentDialogueCallback;
        //public CanvasGroup cg;
        //Test

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }
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
        }

        void Update()
        {
            if (dialoguePanel.activeSelf && Input.GetMouseButtonDown(0))
            {
                /*if (cg != null && cg.alpha < 0.1f)
                {
                    Debug.Log("BBBB");
                    return;
                }
                else
                {
                    Debug.Log("AAAA");
                    Debug.Log(cg.alpha);
                }
                Debug.Log("CCCC");*/
                HandleClick();
            }
        }

        public void StartDialogue(NightDialogueInteractable npc, string[] pages, Action onComplete = null)
        {
            if (pages == null || pages.Length == 0) return;

            currentNPC = npc;
            currentPages = pages;
            currentNames = null;
            currentPageIndex = 0;

            currentDirector = null;
            currentTimelineDirector = null;

            currentDialogueCallback = onComplete;

            dialoguePanel.SetActive(true);
            PlayCurrentPage();
        }

        public void StartDialogue(NightDialogueInteractable npc, string[] pages, string[] dialNames, Action onComplete = null)
        {
            if (pages == null || pages.Length == 0) return;
            /*if (dialNames[currentPageIndex] != null && dialNames.Length > 0) { 
                dialogueName.text = dialNames[currentPageIndex];
                dialogueName.ForceMeshUpdate();
            }*/

            currentNPC = npc;
            currentPages = pages;
            currentNames = dialNames;
            currentPageIndex = 0;

            currentDirector = null;
            currentTimelineDirector = null;

            currentDialogueCallback = onComplete;

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
                    //
                    if (currentNPC != null)
                    {
                        currentNPC.dialogue_sounds_index = 0;
                    }
                    //
                }
            }
        }

        private void PlayCurrentPage()
        {
            string resolvedName = defaultName;

            //TEST
            if (currentNames != null && currentPageIndex < currentNames.Length && !string.IsNullOrEmpty(currentNames[currentPageIndex]))
            {
                resolvedName = currentNames[currentPageIndex];
            }

            // Выводим имя в UI
            if (dialogueName != null)
            {
                dialogueName.text = resolvedName;
                dialogueName.ForceMeshUpdate();
            }
            //TEST

            if (currentNPC != null)
            {
                //
                if (currentNPC.dialogue_sounds_index < currentNPC.dialogue_sounds.Count && currentNPC.dialogue_sounds[currentNPC.dialogue_sounds_index] != null)
                {
                    _audioService.PlaySound(currentNPC.dialogue_sounds[currentNPC.dialogue_sounds_index]);
                }
                currentNPC.dialogue_sounds_index += 1;
            }
            //

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
            currentTimelineDirector = null;
            //dialogueName.ClearMesh();
            dialogueName.text = "";
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

            currentDialogueCallback?.Invoke();
            currentDialogueCallback = null;
        }

        /// <summary>
        /// Completes the dialogue shown by the given timeline as if the player clicked through it.
        /// Used when the cutscene is skipped.
        /// </summary>
        public void CompleteTimelineDialogue(PlayableDirector director)
        {
            if (director == null || currentTimelineDirector != director || !dialoguePanel.activeSelf) return;

            typewriter.StopTyping();
            EndDialogue();
        }

        public void ForceEndDialogue()
        {
            dialoguePanel.SetActive(false);

            typewriter.StopTyping();
        }


        public void StartTimelineDialogue(string[] pages, PlayableDirector director, bool pauseTimeline)
        {
            if (pages == null || pages.Length == 0) return;

            if (CutsceneSkipper.IsSkipping)
            {
                // Skipped cutscene: the dialogue counts as read without being shown.
                DialogEnded?.Invoke();
                return;
            }

            currentTimelineDirector = director;

            currentNPC = null;
            currentPages = pages;
            currentNames = null;
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

        public void StartTimelineDialogue(string[] pages, string[] dialNames, PlayableDirector director, bool pauseTimeline)
        {
            if (pages == null || pages.Length == 0) return;

            if (CutsceneSkipper.IsSkipping)
            {
                // Skipped cutscene: the dialogue counts as read without being shown.
                DialogEnded?.Invoke();
                return;
            }

            currentTimelineDirector = director;

            currentNPC = null;
            currentPages = pages;
            currentNames = dialNames;
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