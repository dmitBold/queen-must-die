using Dialogue;
using TMPro;
using UnityEngine;

namespace NightCycle
{
    public class ThoughtView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] GameObject root; 
        [SerializeField] TextMeshProUGUI textComponent;
        [SerializeField] public TypewriterEffect typewriter;

        [Header("Buttons (Optional)")]
        [SerializeField] GameObject nextButton;
        [SerializeField] GameObject backButton; 
        public void Hide()
        {
            root.SetActive(false);
        }

        public void Show()
        {
            root.SetActive(true);
        }

        public void PlayText(string content)
        {
            typewriter.chunks.Clear();

            typewriter.Play(textComponent, content);

            UpdateButtonsState();
        }

        public TypewriterEffect.SkipResult Skip()
        {
            var result = typewriter.Skip();
            UpdateButtonsState();
            return result;
        }

        public void Back()
        {
            typewriter.back();
            UpdateButtonsState();
        }

        public void UpdateButtonsState()
        {
            if (backButton) backButton.SetActive(typewriter.CanGoBack);
            if (nextButton) nextButton.SetActive(true);
        }
    }
}