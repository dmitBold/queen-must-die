using UnityEngine;
using UnityEngine.UI;


namespace NightCycle.UI
{
    public class BrightnessController : MonoBehaviour
    {
        private const string PrefsKey = "BrightnessSetting";
        private const float DefaultBrightness = 0.5f;

        [Header("UI References")]
        [Tooltip("Слайдер яркости (диапазон 0..1)")]
        [SerializeField] private Slider brightnessSlider;

        [Tooltip("Панель настроек — будет скрыта, если PlayerPrefs уже заданы")]
        [SerializeField] private GameObject settingsPanel;

        [Header("Target Images (необязательно)")]
        [Tooltip("UI Image, яркость которых будет изменяться напрямую через Image.color")]
        [SerializeField] private Image[] targetImages;

        [Header("Brightness Range")]
        [Tooltip("Минимальная яркость (0 = чёрный)")]
        [SerializeField, Range(0f, 1f)] private float minBrightness = 0.0f;

        [Tooltip("Максимальная яркость (1 = белый)")]
        [SerializeField, Range(0f, 1f)] private float maxBrightness = 1.0f;

        [Header("Editor Preview")]
        [Tooltip("Превью яркости прямо в редакторе (без Play Mode). Двигай ползунок — Intensity Multiplier меняется мгновенно.")]
        [SerializeField, Range(0f, 1f)] private float previewBrightness = 0.5f;


        private void Awake()
        {
            bool hasSavedPrefs = PlayerPrefs.HasKey(PrefsKey);
            float saved = PlayerPrefs.GetFloat(PrefsKey, DefaultBrightness);

            ApplyBrightness(saved);
            brightnessSlider.value = saved;
            brightnessSlider.onValueChanged.AddListener(OnSliderChanged);
            settingsPanel.SetActive(!hasSavedPrefs);
        }


        private void OnDestroy()
        {
            brightnessSlider.onValueChanged.RemoveListener(OnSliderChanged);
        }


        private void OnSliderChanged(float normalizedValue)
        {
            ApplyBrightness(normalizedValue);
            PlayerPrefs.SetFloat(PrefsKey, normalizedValue);
            PlayerPrefs.Save();
        }


        private void ApplyBrightness(float normalizedValue)
        {
            float intensity = Mathf.Lerp(minBrightness, maxBrightness, normalizedValue);

            RenderSettings.ambientIntensity = intensity;

            foreach (Image img in targetImages)
            {
                Color c = img.color;
                img.color = new Color(intensity, intensity, intensity, c.a);
            }
        }

        public void ConfirmAndClose()
        {
            settingsPanel.SetActive(false);
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            ApplyBrightness(previewBrightness);
        }
#endif
    }
}
