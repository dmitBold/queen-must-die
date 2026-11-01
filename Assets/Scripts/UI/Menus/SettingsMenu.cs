using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;
using Inventory;
using NightCycle;
using UI;

public class SettingsMenu : MonoBehaviour, IUIScreen
{
    [Tooltip("Идентификатор окна настроек. В ассете включите IsPauseMenu: именно это окно открывает Esc.")]
    [SerializeField] private UIScreenId screenId;

    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Canvas rootCanvas;
    public Camera BrightnessCamera;

    [Header("Save/Load Buttons")]
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _saveButton;

    [SerializeField] private GameObject BrightnessRoot;

    public TMP_Dropdown ResolutionDropdown;


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

    private UIManager _uiManager;

    [Inject]
    public void Construct(SaveManager saveManager, UIManager uiManager)
    {
        _saveManager = saveManager;
        _uiManager = uiManager;
        uiManager.Register(this);
    }

    public UIScreenId Id => screenId;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UnlockButtons();
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

    public void Open()
    {
        isOpen = true;
        panel.SetActive(true);
        PauseGame();
    }

    public void Close()
    {
        isOpen = false;
        panel.SetActive(false);
        BrightnessRoot.SetActive(false);
        BrightnessCamera.gameObject.SetActive(false);
        ResumeGame();
    }

    /// <summary>
    /// Оставлен для кнопок в сцене, назначенных через UnityEvent: делегирует UIManager.
    /// </summary>
    public void Toggle()
    {
        _uiManager.Toggle(screenId);
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
        LockButtons();
        _saveManager.LoadGame();
    }

    public void SaveOnExit()
    {
        LockButtons();
        _saveManager.SaveGame();
        QuitGame();
    }

    private void LockButtons()
    {
        if (_loadButton != null) _loadButton.interactable = false;
        if (_saveButton != null) _saveButton.interactable = false;
    }

    private void UnlockButtons()
    {
        if (_loadButton != null) _loadButton.interactable = true;
        if (_saveButton != null) _saveButton.interactable = true;
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

}
