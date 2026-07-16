using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Options : MonoBehaviour
{
    private const float TransitionDuration = 0.4f;

    public static bool PlayIntroOnAwake { get; set; }

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

    private RectTransform rootRectTransform;
    private readonly List<RectTransform> transitionTargets = new List<RectTransform>();
    private readonly List<Vector2> defaultAnchoredPositions = new List<Vector2>();
    private Sequence transitionSequence;
    private bool isTransitioning;
    private InputAction backInputAction;

    private void Awake()
    {
        rootRectTransform = (RectTransform)transform;
        var shouldPlayIntro = PlayIntroOnAwake;
        PlayIntroOnAwake = false;

        CacheTransitionTargets();

        if (shouldPlayIntro)
        {
            PrepareIntroPosition();
        }

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

    public Tween PlayIntroTransition(float duration)
    {
        isTransitioning = true;
        transitionSequence?.Kill();
        transitionSequence = CreateMoveSequence(Vector2.zero, duration);
        transitionSequence.OnComplete(() =>
        {
            transitionSequence = null;
            isTransitioning = false;
        });
        return transitionSequence;
    }

    private void OpenResetConfirmation()
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
        resetConfirmationPopup.Open(ConfirmReset);
    }

    private void ConfirmReset()
    {
        SaveManager.Reset();
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
            var lobbyScreen = FindFirstObjectByType<UI_Lobby>();
            transitionSequence?.Kill();
            transitionSequence = DOTween.Sequence().SetTarget(this).SetLink(gameObject);
            transitionSequence.Join(lobbyScreen.PlayReturnTransition(TransitionDuration));
            transitionSequence.Join(CreateMoveSequence(Vector2.right * GetTransitionOffset(), TransitionDuration));
            transitionSequence.OnComplete(() => SceneManager.UnloadSceneAsync(Definitions.OptionSceneName));
            return;
        }

        gameObject.SetActive(false);
    }

    private void PrepareIntroPosition()
    {
        var introOffset = Vector2.right * GetTransitionOffset();
        for (var i = 0; i < transitionTargets.Count; i++)
        {
            transitionTargets[i].anchoredPosition = defaultAnchoredPositions[i] + introOffset;
        }
    }

    private Sequence CreateMoveSequence(Vector2 offset, float duration)
    {
        var sequence = DOTween.Sequence().SetTarget(this).SetLink(gameObject);
        for (var i = 0; i < transitionTargets.Count; i++)
        {
            sequence.Join(transitionTargets[i].DOAnchorPos(defaultAnchoredPositions[i] + offset, duration)
                .SetEase(Ease.InOutCubic)
                .SetTarget(transitionTargets[i])
                .SetLink(transitionTargets[i].gameObject));
        }

        return sequence;
    }

    private void CacheTransitionTargets()
    {
        transitionTargets.Clear();
        defaultAnchoredPositions.Clear();

        foreach (Transform child in transform)
        {
            var rectTarget = (RectTransform)child;
            transitionTargets.Add(rectTarget);
            defaultAnchoredPositions.Add(rectTarget.anchoredPosition);
        }
    }

    private float GetTransitionOffset()
    {
        return rootRectTransform.rect.width > 0f ? rootRectTransform.rect.width : Screen.width;
    }

    private void OnDestroy()
    {
        DisplayModeManager.Changed -= RefreshDisplayModeButtons;
        transitionSequence?.Kill();
        backInputAction.Dispose();
    }
}
