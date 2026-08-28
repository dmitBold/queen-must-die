using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NightCycle
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] TMP_Text interactionText;
        public static HUDController instance;
        [SerializeField] public Image CrosshairImage;
        [SerializeField] Image interactionProgressImage;
        public Sprite DefaultImage;

        private void Awake()
        {
            instance = this;
            CrosshairImage.rectTransform.sizeDelta /= 2f;
            DefaultImage = CrosshairImage.sprite;
            SetDefaultImage();
            SetCrosshairActivity(false);
        }

        public void EnableInteractionText(string text)
        {
            interactionText.text = text + " (E)";
            interactionText.gameObject.SetActive(true);
        }

        public void DisableInteractionText()
        {
            interactionText.gameObject.SetActive(false);
        }

        public void ChangeCrosshairImage(Sprite sprite)
        {
            CrosshairImage.sprite = sprite;
        }
        
        public void SetCrosshairActivity(bool isActive)
        {
            CrosshairImage.gameObject.SetActive(isActive);
        }

        private void Start()
        {
            if (interactionProgressImage != null)
            {
                interactionProgressImage.fillAmount = 0;
                interactionProgressImage.gameObject.SetActive(false);
            }
        }

        public void UpdateProgress(float percentage)
        {
            if (interactionProgressImage == null) return;

            interactionProgressImage.gameObject.SetActive(true);
            interactionProgressImage.fillAmount = percentage;
        }

        public void HideProgress()
        {
            if (interactionProgressImage == null) return;

            interactionProgressImage.fillAmount = 0;
            interactionProgressImage.gameObject.SetActive(false);
        }

        public void SetDefaultImage()
        {
            ChangeCrosshairImage(DefaultImage);
        }

    }
}
