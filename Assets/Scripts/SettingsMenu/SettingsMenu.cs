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

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Canvas rootCanvas;
    public Camera BrightnessCamera;

    [Header("Save/Load Buttons")]
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _saveButton;

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

    private AssemblyService _assemblyService;
    private InventoryUI _inventoryUI;
    private bool invUI_Prev_state; 

    [Inject]
    public void Construct(SaveManager saveManager, AssemblyService assemblyService, InventoryUI inventoryUI)
    {
        _saveManager = saveManager;
        _assemblyService = assemblyService;
        _inventoryUI = inventoryUI;
    }

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

        // Если при смене сцены меню паузы осталось открытым - закрываем его,
        // чтобы снять паузу (Time.timeScale = 1) и спрятать UI.
        if (isOpen)
        {
            Close();
        }
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
        Debug.Log("ASS" + _assemblyService.IsActive);
        Debug.Log("INV" + _inventoryUI.isOpen);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle();
        }
    }

    private void HandleEscapeInput()
    {
        if (isOpen)
        {
            Close();
            return;
        }

        if (_assemblyService != null && _assemblyService.IsActive)
        {
            _assemblyService.CloseAssembly();
            return; // Выходим, чтобы не открыть меню паузы!
        }

        // 2. Затем проверяем инвентарь
        if (_inventoryUI != null && _inventoryUI.isOpen)
        {
            _inventoryUI.Close();
            return;
        }
        /*if (_inventoryUI != null && _inventoryUI.isOpen)
        {
            if (_assemblyService != null && _assemblyService.IsActive)
            {
                _assemblyService.CloseAssembly();
            }
            _inventoryUI.Close();
            return;
        }
        Debug.Log(_assemblyService.IsActive);
        if (_assemblyService != null && _assemblyService.IsActive)
        {
            _assemblyService.CloseAssembly();
            return;
        }*/

        Open();
    }

    public void Toggle()
    {
        if (!CanOpen())
            return;

        /*if (isOpen)
            Close();
        else
            Open();*/
        HandleEscapeInput();
    }

    private bool CanOpen()
    {
        return true;
    }

    private void Open()
    {
        if (_inventoryUI != null && _inventoryUI.isOpen)
        {
            _inventoryUI.Close();
            //invUI_Prev_state = _inventoryUI.canOpen;
        }
        _inventoryUI.canOpen = false;
        isOpen = true;
        panel.SetActive(true);
        PauseGame();
    }

    private void Close()
    {
        //_inventoryUI.canOpen = invUI_Prev_state;
        if (_assemblyService.IsActive)
        {
            _inventoryUI.canOpen = false;
        }
        else
        {
            _inventoryUI.canOpen = true;
        }
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
