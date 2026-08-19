using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Zenject;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Canvas rootCanvas;
    public Camera BrightnessCamera;

    [SerializeField] private GameObject BrightnessRoot;

    public TMP_Dropdown ResolutionDropdown;

    private CursorLockMode previousLockState;
    private bool previousVisibleState;

    [Header("State")] 
    public bool isOpen = false;
    private bool isPaused = false;

    Resolution[] resolutions;

    private SaveManager _saveManager;

    [Header("Mouse Sensitivity")]
    [SerializeField] private Slider sensitivitySlider;
    private const string SensKey = "MouseSensitivity";
    public static event System.Action<float> OnSensitivityChanged;

    [Header("Retro Graphics")]
    [SerializeField] private Toggle retroToggle;
    [SerializeField] private UniversalRendererData rendererData;
    [SerializeField] private string retroFeatureName = "FullScreenPassRendererFeature";

    [Inject]
    public void Construct(SaveManager saveManager)
    {
        _saveManager = saveManager;
    }

    private void Start()
    {
        SetUPResolution();

        // 1. Инициализация чувствительности
        float savedSens = PlayerPrefs.GetFloat(SensKey, 1.0f);
        sensitivitySlider.value = savedSens;
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);

        // 2. Инициализация ретро-шейдера
        bool isRetroOn = PlayerPrefs.GetInt("RetroShader", 1) == 1;
        retroToggle.isOn = isRetroOn;
        SetRetroEffect(isRetroOn);
        retroToggle.onValueChanged.AddListener(SetRetroEffect);
    }

    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat(SensKey, value);
        OnSensitivityChanged?.Invoke(value);
    }

    public void SetRetroEffect(bool isOn)
    {
        PlayerPrefs.SetInt("RetroShader", isOn ? 1 : 0);

        if (rendererData != null)
        {
            var feature = rendererData.rendererFeatures.Find(f => f.name == retroFeatureName);
            if (feature != null)
            {
                feature.SetActive(isOn);
            }
            else
            {
                Debug.LogWarning($"Render feature {retroFeatureName} не найдена!");
            }
        }
    }
    private void OnDestroy()
    {
        sensitivitySlider.onValueChanged.RemoveListener(SetSensitivity);
        retroToggle.onValueChanged.RemoveListener(SetRetroEffect);
    }

    public void SetUPResolution()
    {
        int CurrentResolutionIndex = 0;

        resolutions = Screen.resolutions;

        ResolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                CurrentResolutionIndex = i;
            }
        }

        ResolutionDropdown.AddOptions(options);
        ResolutionDropdown.value = CurrentResolutionIndex;
        ResolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];

        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void DisableRootCanvas()
    {
        rootCanvas.gameObject.SetActive(false);
    }

    public void EnableRootCanvas()
    {
        rootCanvas.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (!CanOpen())
            return;

        if (isOpen)
            Close();
        else
            Open();
    }

    private bool CanOpen()
    {
        return true;
    }

    private void Open()
    {
        isOpen = true;
        panel.SetActive(true);
        PauseGame();
    }

    private void Close()
    {
        isOpen = false;
        panel.SetActive(false);
        BrightnessRoot.SetActive(false);
        BrightnessCamera.gameObject.SetActive(false);
        ResumeGame();
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void LoadLastSave()
    {
        _saveManager.LoadGame();
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        previousLockState = Cursor.lockState;
        previousVisibleState = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        Cursor.lockState = previousLockState;
        Cursor.visible = previousVisibleState;
    }

}
