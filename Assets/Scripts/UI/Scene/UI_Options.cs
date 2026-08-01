using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Options : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private InputActionReference backAction;
    
    [Header("Display")]
    [SerializeField] private Button windowButton;
    [SerializeField] private Button fullscreenButton;
    [SerializeField] private Button resolutionLeftButton;
    [SerializeField] private Button resolutionRightButton;
    [SerializeField] private TMP_Text resolutionValue;
    
    [Header("Sound")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TMP_Text bgmValue;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxValue;
    
    [Header("Localization")]
    [SerializeField] private Transform languageButtons;
    [SerializeField] private UI_LanguageButton languageButton;
    
    [Header("Savefile")]
    [SerializeField] private Button resetButton;
    [SerializeField] private UI_ResetConfirmationPopup resetConfirmationPopupPrefab;

    private UI_ResetConfirmationPopup resetConfirmationPopup;

    private bool isTransitioning;
    private InputAction backInputAction;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        backInputAction = backAction.action.Clone();
        resetButton.onClick.AddListener(OpenResetConfirmation);
        bgmSlider.onValueChanged.AddListener(OnBgmSlider);
        sfxSlider.onValueChanged.AddListener(OnSfxSlider);
        windowButton.onClick.AddListener(() => SetFullScreen(false));
        fullscreenButton.onClick.AddListener(() => SetFullScreen(true));
        resolutionLeftButton.onClick.AddListener(OnResolutionLeftButton);
        resolutionRightButton.onClick.AddListener(OnResolutionRightButton);
        DisplayModeManager.Changed += RefreshDisplayModeButtons;
        InitLanguageButtons();
        resetConfirmationPopup = Instantiate(resetConfirmationPopupPrefab, transform);

        Open();
    }

    private void OnEnable()
    {
        backInputAction.Enable();
    }

    private void OnDisable()
    {
        backInputAction.Disable();
    }

    private void Update()
    {
        if (backInputAction.WasPressedThisFrame())
        {
            if (resetConfirmationPopup.IsOpen)
            {
                CloseResetConfirmation();
                return;
            }

            Close();
        }
    }

    private void Open()
    {
        bgmSlider.SetValueWithoutNotify(GameManager.Instance.Sound.BgmVolume);
        sfxSlider.SetValueWithoutNotify(GameManager.Instance.Sound.SfxVolume);
        UpdateBgmValue(GameManager.Instance.Sound.BgmVolume);
        UpdateSfxValue(GameManager.Instance.Sound.SfxVolume);
        RefreshDisplayModeButtons();
        gameObject.SetActive(true);
    }

    private void OpenResetConfirmation()
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
        resetConfirmationPopup.Open(ConfirmReset);
    }

    private void ConfirmReset()
    {
        SaveManager.Reset();
        GameManager.Instance.ResetStageSelection();
        ReturnToGameStartScreen();
    }

    private void CloseResetConfirmation()
    {
        resetConfirmationPopup.Close();
    }

    private void OnBgmSlider(float value)
    {
        GameManager.Instance.Sound.SetBgmVolume(value);
        UpdateBgmValue(value);
    }

    private void OnSfxSlider(float value)
    {
        GameManager.Instance.Sound.SetSfxVolume(value);
        UpdateSfxValue(value);
    }

    private void UpdateBgmValue(float value)
    {
        bgmValue.text = Mathf.RoundToInt(value * 100f).ToString();
    }

    private void UpdateSfxValue(float value)
    {
        sfxValue.text = Mathf.RoundToInt(value * 100f).ToString();
    }
    
    private void SetFullScreen(bool fullScreen)
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
        DisplayModeManager.SetFullScreen(fullScreen);
    }

    private void OnResolutionLeftButton()
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
        DisplayModeManager.SelectPreviousResolution();
    }

    private void OnResolutionRightButton()
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
        DisplayModeManager.SelectNextResolution();
    }

    private void RefreshDisplayModeButtons()
    {
        windowButton.interactable = DisplayModeManager.IsFullScreen;
        fullscreenButton.interactable = !DisplayModeManager.IsFullScreen;
        resolutionLeftButton.interactable = DisplayModeManager.CanSelectPreviousResolution;
        resolutionRightButton.interactable = DisplayModeManager.CanSelectNextResolution;
        resolutionValue.text = DisplayModeManager.ResolutionLabel;
    }

    private void InitLanguageButtons()
    {
        for (var index = 0; index < Localization.SupportedLocales.Length; index++)
        {
            var button = Instantiate(languageButton, languageButtons);
            button.Init(index);
        }
    }

    private void Close()
    {
        if (isTransitioning)
        {
            return;
        }

        ReturnToGameStartScreen();
    }

    private void ReturnToGameStartScreen()
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
        if (gameObject.scene.name == Definitions.OptionSceneName)
        {
            isTransitioning = true;
            FindFirstObjectByType<UI_Lobby>().RestoreAfterOptionsClose();
            SceneManager.UnloadSceneAsync(Definitions.OptionSceneName);
            return;
        }

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        DisplayModeManager.Changed -= RefreshDisplayModeButtons;
        backInputAction.Dispose();
    }
}
