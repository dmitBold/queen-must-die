using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

        [Header("Brightness Range (Multiplier)")]
        [Tooltip("Минимальный множитель яркости (0.2 = 20% от базовой сцены)")]
        [SerializeField, Range(0f, 2f)] private float minMultiplier = 0.2f;

        [Tooltip("Максимальный множитель яркости (2.0 = 200% от базовой сцены)")]
        [SerializeField, Range(0f, 3f)] private float maxMultiplier = 2.0f;

        [Header("Editor Preview")]
        [Tooltip("Превью яркости в Play Mode")]
        [SerializeField, Range(0f, 1f)] private float previewBrightness = 0.5f;

        private float _baseAmbientIntensity;
        private Color[] _baseImageColors;
        private float _currentNormalizedValue;

        private void Awake()
        {
            bool hasSavedPrefs = PlayerPrefs.HasKey(PrefsKey);
            _currentNormalizedValue = hasSavedPrefs
                ? PlayerPrefs.GetFloat(PrefsKey, DefaultBrightness)
                : DefaultBrightness;

            if (brightnessSlider != null)
            {
                brightnessSlider.value = _currentNormalizedValue;
                brightnessSlider.onValueChanged.AddListener(OnSliderChanged);
            }

            if (hasSavedPrefs && settingsPanel != null)
                settingsPanel.SetActive(false);

            if (targetImages != null)
            {
                _baseImageColors = new Color[targetImages.Length];
                for (int i = 0; i < targetImages.Length; i++)
                {
                    if (targetImages[i] != null)
                        _baseImageColors[i] = targetImages[i].color;
                }
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (brightnessSlider != null)
                brightnessSlider.onValueChanged.RemoveListener(OnSliderChanged);
        }

        private void Start()
        {
            CaptureBaseLighting();
            ApplyBrightness(_currentNormalizedValue);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CaptureBaseLighting();
            ApplyBrightness(_currentNormalizedValue);
        }

        private void CaptureBaseLighting()
        {
            _baseAmbientIntensity = RenderSettings.ambientIntensity;
        }

        private void OnSliderChanged(float normalizedValue)
        {
            _currentNormalizedValue = normalizedValue;
            ApplyBrightness(normalizedValue);
            PlayerPrefs.SetFloat(PrefsKey, normalizedValue);
            PlayerPrefs.Save();
        }

        private void ApplyBrightness(float normalizedValue)
        {
            float multiplier = Mathf.Lerp(minMultiplier, maxMultiplier, normalizedValue);
            RenderSettings.ambientIntensity = _baseAmbientIntensity * multiplier;

            if (targetImages == null) return;

            for (int i = 0; i < targetImages.Length; i++)
            {
                if (targetImages[i] == null) continue;
                Color baseColor = _baseImageColors[i];
                Color newColor = baseColor * multiplier;
                newColor.a = baseColor.a;
                targetImages[i].color = newColor;
            }
        }

        public void ConfirmAndClose()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying) return;
            ApplyBrightness(previewBrightness);
        }
#endif
    }
}
