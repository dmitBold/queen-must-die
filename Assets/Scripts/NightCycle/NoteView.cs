using Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NightCycle
{
    public class NoteView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] GameObject root;
        //test
        [SerializeField] public GameObject Tintroot;
        bool show_tint;
        string[] tint_text;
        //test
        [SerializeField] Image noteImageDisplay;
        [SerializeField] TextMeshProUGUI textComponent;
        [SerializeField] public TypewriterEffect typewriter;

        [Header("Buttons (Optional)")]
        [SerializeField] GameObject nextButton;
        [SerializeField] GameObject backButton;

        public void Show(Sprite image, string[] content)
        {
            root.SetActive(true);
            //test
            Tintroot.SetActive(false);
            show_tint = true;

            if (noteImageDisplay != null)
            {
                noteImageDisplay.sprite = image;
                noteImageDisplay.enabled = (image != null);
            }

            tint_text = content;
            //реяр!!! врнаш рейяр гюохяйх ндмнбпелеммн бйкчвюкяъ я йюпрхмйни. пюмэье щрнцн акнйю йндю ме ашкн!!
            Tintroot.SetActive(true);
            typewriter.Play(textComponent, tint_text);
            UpdateButtonsState();
            show_tint = false;
            //реяр!!! врнаш рейяр гюохяйх ндмнбпелеммн бйкчвюкяъ я йюпрхмйни. пюмэье щрнцн акнйю йндю ме ашкн!!
        }

        public void Hide()
        {
            root.SetActive(false);
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

        private void Update()
        {
            if (!root.activeSelf) return;
            if (Input.GetMouseButtonDown(0))
            {
                if (show_tint)
                {
                    Tintroot.SetActive(true);
                    //typewriter.chunks.Clear();
                    typewriter.Play(textComponent, tint_text);

                    UpdateButtonsState();
                    show_tint = false;
                }
                //else
                //{
                //Tintroot.SetActive(false);
                //show_tint = true;
                //}
            }
        }
    }
}