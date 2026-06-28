using TMPro;
using UnityEngine;

namespace Dialogue
{
    public class DialogueView : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] TextMeshProUGUI name_text;
        [SerializeField] public TypewriterEffect typewriter;
        [SerializeField] GameObject skipButton;
        [SerializeField] GameObject backButton;

        public void Show() => root.SetActive(true);
        public void Hide() => root.SetActive(false);

        public void PlayText(string[] contentPages)
        {
            skipButton.SetActive(false);
            backButton.SetActive(false);
            typewriter.Play(text, contentPages);
        }

        public void SetSkipVisible(bool visible)
        {
            skipButton.SetActive(visible);
        }

        public void SetBackVisible(bool visible)
        {
            backButton.SetActive(visible);
        }

        public TypewriterEffect.SkipResult Skip()
        {
            return typewriter.Skip();
        }

        public bool canSkip()
        {
            return skipButton.activeSelf;
        }

        public void set_name(string name)
        {
            name_text.text = name;
        }

        public bool IsFinished => typewriter.IsFinished;
    }
}