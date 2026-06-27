using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Option : MonoBehaviour
{
    private const float TransitionDuration = 0.4f;

    public static bool PlayIntroOnAwake { get; set; }

    [SerializeField] private Button closeButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button completeButton;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TMP_Text bgmValue;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxValue;
    [SerializeField] private Transform languageButtons;
    [SerializeField] private Button languageButton;

    private RectTransform rootRectTransform;
    private readonly List<RectTransform> transitionTargets = new List<RectTransform>();
    private readonly List<Vector2> defaultAnchoredPositions = new List<Vector2>();
    private Sequence transitionSequence;
    private bool isTransitioning;

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
        resetButton.onClick.AddListener(SaveManager.Reset);
        completeButton.onClick.AddListener(SaveManager.CompleteAllStages);
        bgmSlider.onValueChanged.AddListener(OnBgmSlider);
        sfxSlider.onValueChanged.AddListener(OnSfxSlider);
        InitLanguageButtons();

        Open();
    }

    public void Open()
    {
        bgmSlider.SetValueWithoutNotify(GameManager.Instance.Sound.BgmVolume);
        sfxSlider.SetValueWithoutNotify(GameManager.Instance.Sound.SfxVolume);
        UpdateBgmValue(GameManager.Instance.Sound.BgmVolume);
        UpdateSfxValue(GameManager.Instance.Sound.SfxVolume);
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

    private void InitLanguageButtons()
    {
        for (var i = 0; i < Localization.SupportedLocales.Length; i++)
        {
            var button = Instantiate(languageButton, languageButtons);
            var locale = Localization.SupportedLocales[i];
            button.GetComponentInChildren<TMP_Text>().text = Localization.SupportedLocaleLabels[i];
            button.onClick.AddListener(() => OnLanguageButton(locale));
        }

        RefreshLanguageButtons();
    }

    private void OnLanguageButton(string locale)
    {
        GameManager.Instance.Localization.SetLocale(locale);
        RefreshLanguageButtons();
    }

    private void RefreshLanguageButtons()
    {
        for (var i = 0; i < Localization.SupportedLocales.Length; i++)
        {
            var button = languageButtons.GetChild(i).GetComponent<Button>();
            button.interactable = Localization.SupportedLocales[i] != GameManager.Instance.Localization.CurrentLocale;
        }
    }

    private void Close()
    {
        if (isTransitioning)
        {
            return;
        }

        if (gameObject.scene.name == Definitions.OptionSceneName)
        {
            isTransitioning = true;
            var lobbyScreen = FindObjectOfType<UI_LobbyScreen>();
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
        transitionSequence?.Kill();
    }
}
