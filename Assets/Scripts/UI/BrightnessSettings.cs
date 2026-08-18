using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Zenject;

public class BrightnessSettings : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider brightnessSlider;
    public Image brightnessRoot;

    [Header("Post Processing")]
    public Volume globalVolume;
    //[Inject]
    //private Volume globalVolume;

    private ColorAdjustments colorAdjustments;
    private const string BrightnessKey = "GameExposure"; // Ключ для сохранения

    private bool is_root_active = false;

    void Start()
    {
        // Пытаемся получить доступ к Color Adjustments внутри профиля
        if (globalVolume.profile.TryGet(out colorAdjustments))
        {
            // Загружаем сохраненное значение (если его нет, берем 0)
            float savedBrightness = PlayerPrefs.GetFloat(BrightnessKey, 0f);

            // Устанавливаем ползунок на нужную позицию
            brightnessSlider.value = savedBrightness;

            // Применяем яркость
            SetBrightness(savedBrightness);
        }
        else
        {
            Debug.LogError("В Global Volume нет Color Adjustments!");
        }

        // Подписываемся на изменение ползунка
        brightnessSlider.onValueChanged.AddListener(SetBrightness);
    }

    public void SetBrightness(float value)
    {
        if (colorAdjustments != null)
        {
            // Меняем значение экспозиции
            colorAdjustments.postExposure.value = value;

            // Сохраняем настройку в память устройства
            PlayerPrefs.SetFloat(BrightnessKey, value);
        }
    }

    void OnDestroy()
    {
        // Очищаем подписку при закрытии меню
        brightnessSlider.onValueChanged.RemoveListener(SetBrightness);
        // Физически записываем данные на диск
        PlayerPrefs.Save();
    }

    public void Toggle()
    {
        if (is_root_active)
        {
            brightnessRoot.gameObject.SetActive(false);
            is_root_active=false;
        }
        else
        {
            brightnessRoot.gameObject.SetActive(true);
            is_root_active = true;
        }
    }

}