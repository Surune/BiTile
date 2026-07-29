using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Lobby : MonoBehaviour
{
    private const float TransitionDuration = 0.4f;
    private const float LoadFadeInDuration = 0.5f;

    public static bool OpenStageSelectOnAwake { get; set; }
    public static bool OpenChapterSelectOnAwake { get; set; }

    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private TMP_Text versionText;
    [SerializeField] private InputActionReference confirmAction;

    private Button hardModeButton;
    private readonly List<RectTransform> transitionTargets = new List<RectTransform>();
    private readonly List<Vector2> defaultAnchoredPositions = new List<Vector2>();
    private Sequence transitionSequence;
    private Tween loadFadeInTween;
    private Image loadFadeInOverlay;
    private bool isTransitioning;
    private bool openStageSelectImmediatelyOnAwake;
    private InputAction confirmInputAction;

    private void Awake()
    {
        CreateModeButtons();
        CacheTransitionTargets();

        quitButton.onClick.AddListener(Application.Quit);
        startButton.onClick.AddListener(() => OnGameStart(Definitions.GameMode.Normal));
        hardModeButton.onClick.AddListener(() => OnGameStart(Definitions.GameMode.Hard));
        optionButton.onClick.AddListener(OnOptionButton);

        versionText.text = $"{Application.version}({BuildInfo.GitHash})";

        confirmInputAction = confirmAction.action.Clone();

        GameManager.Instance.Sound.PlayBGM(Definitions.SoundType.Bgm);

        openStageSelectImmediatelyOnAwake = OpenStageSelectOnAwake;
        OpenStageSelectOnAwake = false;
        var openChapterSelectImmediatelyOnAwake = OpenChapterSelectOnAwake;
        OpenChapterSelectOnAwake = false;
        if (openStageSelectImmediatelyOnAwake)
        {
            OpenStageSelectWithFade();
            return;
        }

        if (openChapterSelectImmediatelyOnAwake)
        {
            OpenChapterSelectImmediately();
            return;
        }

        PlayLoadFadeIn();
    }

    private void OnEnable()
    {
        confirmInputAction.performed += OnConfirmAction;
        confirmInputAction.Enable();
    }

    private void OnDisable()
    {
        confirmInputAction.performed -= OnConfirmAction;
        confirmInputAction.Disable();
    }

    private void OnConfirmAction(InputAction.CallbackContext context)
    {
        if (SceneManager.GetSceneByName(Definitions.OptionSceneName).isLoaded ||
            SceneManager.GetSceneByName(Definitions.ChapterSelectSceneName).isLoaded ||
            SceneManager.GetSceneByName(Definitions.StageSelectSceneName).isLoaded)
        {
            return;
        }

        OnGameStart(Definitions.GameMode.Normal);
    }

    private void OnGameStart(Definitions.GameMode mode)
    {
        if (isTransitioning)
        {
            return;
        }

        GameManager.Instance.SetMode(mode);
        isTransitioning = true;
        canvasGroup.blocksRaycasts = false;

        UI_ChapterSelect.PlayIntroOnAwake = true;
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.GameStart);
        var loadOperation = SceneManager.LoadSceneAsync(Definitions.ChapterSelectSceneName, LoadSceneMode.Additive);
        loadOperation.completed += _ => PlayChapterSelectTransition();
    }

    private void CreateModeButtons()
    {
        var buttonParent = startButton.transform.parent;
        var siblingIndex = startButton.transform.GetSiblingIndex();

        var modeButtons = new GameObject("ModeButtons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        modeButtons.layer = gameObject.layer;
        var modeButtonsRectTransform = (RectTransform)modeButtons.transform;
        modeButtonsRectTransform.SetParent(buttonParent, false);
        modeButtonsRectTransform.SetSiblingIndex(siblingIndex);
        modeButtonsRectTransform.sizeDelta = new Vector2(0f, 175f);

        startButton.transform.SetParent(modeButtonsRectTransform, false);
        startButton.GetComponentInChildren<UI_LocalizedText>().SetLKey(Definitions.LKey.UI_NORMAL_MODE);

        hardModeButton = Instantiate(startButton, modeButtonsRectTransform);
        hardModeButton.name = "HardModeButton";
        hardModeButton.GetComponentInChildren<UI_LocalizedText>().SetLKey(Definitions.LKey.UI_HARD_MODE);
        RefreshModeButtons();

        var layoutGroup = modeButtons.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 20f;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = true;
    }

    public void RefreshModeButtons()
    {
        hardModeButton.interactable = SaveManager.IsHardModeUnlocked();
        hardModeButton.GetComponentInChildren<TMP_Text>().color =
            hardModeButton.interactable ? Color.white : Color.gray;
    }

    private void OnOptionButton()
    {
        if (isTransitioning || SceneManager.GetSceneByName(Definitions.OptionSceneName).isLoaded)
        {
            return;
        }

        isTransitioning = true;
        canvasGroup.blocksRaycasts = false;

        UI_Options.PlayIntroOnAwake = true;
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
        var loadOperation = SceneManager.LoadSceneAsync(Definitions.OptionSceneName, LoadSceneMode.Additive);
        loadOperation.completed += _ => PlayOptionTransition();
    }

    private void PlayChapterSelectTransition()
    {
        var chapterSelect = FindFirstObjectByType<UI_ChapterSelect>();
        transitionSequence = CreateMoveSequence(Vector2.up * GetTransitionOffset(), TransitionDuration);
        transitionSequence.Join(chapterSelect.PlayIntroTransition(TransitionDuration));
        transitionSequence.OnComplete(() =>
        {
            transitionSequence = null;
            isTransitioning = false;
        });
    }

    private void OpenStageSelectWithFade()
    {
        isTransitioning = true;
        canvasGroup.blocksRaycasts = false;
        loadFadeInOverlay = CreateLoadFadeInOverlay();
        SetTransitionPosition(Vector2.up * GetTransitionOffset());
        UI_ChapterSelect.PlayIntroOnAwake = false;
        var loadOperation = SceneManager.LoadSceneAsync(Definitions.ChapterSelectSceneName, LoadSceneMode.Additive);
        loadOperation.completed += _ =>
        {
            var chapterSelect = FindFirstObjectByType<UI_ChapterSelect>();
            var stageLoadOperation = chapterSelect.OpenStageSelectImmediately(GameManager.Instance.StageSelection.Chapter);
            stageLoadOperation.completed += _ => PlayStageSelectFadeIn();
        };
    }

    private void PlayStageSelectFadeIn()
    {
        loadFadeInTween = loadFadeInOverlay.DOFade(0f, LoadFadeInDuration)
            .SetEase(Ease.OutCubic)
            .SetTarget(this)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                Destroy(loadFadeInOverlay.gameObject);
                loadFadeInTween = null;
                openStageSelectImmediatelyOnAwake = false;
                isTransitioning = false;
            });
    }

    private void OpenChapterSelectImmediately()
    {
        isTransitioning = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0f;
        SetTransitionPosition(Vector2.up * GetTransitionOffset());
        UI_ChapterSelect.PlayIntroOnAwake = false;
        SceneManager.LoadSceneAsync(Definitions.ChapterSelectSceneName, LoadSceneMode.Additive);
    }

    private void PlayLoadFadeIn()
    {
        isTransitioning = true;
        canvasGroup.blocksRaycasts = false;
        loadFadeInOverlay = CreateLoadFadeInOverlay();
        loadFadeInTween = loadFadeInOverlay.DOFade(0f, LoadFadeInDuration)
            .SetEase(Ease.OutCubic)
            .SetTarget(this)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                Destroy(loadFadeInOverlay.gameObject);
                loadFadeInTween = null;
                canvasGroup.blocksRaycasts = true;
                isTransitioning = false;
            });
    }

    private Image CreateLoadFadeInOverlay()
    {
        var overlay = new GameObject("Load Fade In Overlay");
        overlay.layer = gameObject.layer;
        overlay.transform.SetParent(transform, false);

        var rectTransform = overlay.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        var image = overlay.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = true;

        var canvas = overlay.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = short.MaxValue;
        var overlayCanvasGroup = overlay.AddComponent<CanvasGroup>();
        overlayCanvasGroup.ignoreParentGroups = true;
        overlay.AddComponent<GraphicRaycaster>();
        overlay.transform.SetAsLastSibling();
        return image;
    }

    private void SetTransitionPosition(Vector2 offset)
    {
        for (var i = 0; i < transitionTargets.Count; i++)
        {
            transitionTargets[i].anchoredPosition = defaultAnchoredPositions[i] + offset;
        }
    }

    private void PlayOptionTransition()
    {
        var option = FindFirstObjectByType<UI_Options>();
        transitionSequence = CreateMoveSequence(Vector2.left * GetHorizontalTransitionOffset(), TransitionDuration);
        transitionSequence.Join(option.PlayIntroTransition(TransitionDuration));
        transitionSequence.OnComplete(() =>
        {
            transitionSequence = null;
            isTransitioning = false;
        });
    }

    public Tween PlayReturnTransition(float duration)
    {
        isTransitioning = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 1f;
        transitionSequence?.Kill();
        transitionSequence = CreateMoveSequence(Vector2.zero, duration);
        transitionSequence.OnComplete(() =>
        {
            transitionSequence = null;
            canvasGroup.blocksRaycasts = true;
            isTransitioning = false;
        });
        return transitionSequence;
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
        return canvasRectTransform.rect.height > 0f ? canvasRectTransform.rect.height : Screen.height;
    }

    private float GetHorizontalTransitionOffset()
    {
        return canvasRectTransform.rect.width > 0f ? canvasRectTransform.rect.width : Screen.width;
    }

    private void OnDestroy()
    {
        loadFadeInTween?.Kill();
        transitionSequence?.Kill();
        confirmInputAction.Dispose();
    }
}
