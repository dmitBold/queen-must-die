using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace NightCycle
{
    public class DayDialogueManager : MonoBehaviour
    {

        [Header("UI")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private NightTypewriter typewriter;
        public event System.Action OnDialogueEnded;

        private string[] currentPages;
        private int currentPageIndex;

        void Update()
        {
            if (dialoguePanel.activeSelf && Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
        }

        public void StartDialogue(string[] pages)
        {
            if (pages == null || pages.Length == 0) return;

            currentPages = pages;
            currentPageIndex = 0;


            dialoguePanel.SetActive(true);
            PlayCurrentPage();
        }

        private void HandleClick()
        {
            //return;
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
                }
            }
        }

        private void PlayCurrentPage()
        {
            typewriter.TypeText(currentPages[currentPageIndex]);
        }


        public void EndDialogue()
        {
            //dialoguePanel.SetActive(false);
            OnDialogueEnded?.Invoke();
        }

        public void Hide()
        {
            dialoguePanel.SetActive(false);
        }

    }
}