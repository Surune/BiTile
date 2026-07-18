using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class UI_ChapterSelect : MonoBehaviour
{
    private const float TransitionDuration = 0.4f;
    private const float ChapterExitDistance = 7f;
    private static readonly Color SelectedModeColor = new Color(1f, 0.7411765f, 0.2196078f);

    public static bool OpenStageSelectOnAwake { get; set; }

    [SerializeField] private UI_ChapterCarousel chapterCarousel;
    [SerializeField] private Camera backgroundCamera;
    [SerializeField] private float chapterExitDistance = ChapterExitDistance;
    [SerializeField] private InputActionReference backAction;
    [SerializeField] private InputActionReference confirmAction;
    [SerializeField] private Button backButton;
    [SerializeField] private Button normalModeButton;
    [SerializeField] private Button hardModeButton;
    [SerializeField] private RectTransform modeButtonsRectTransform;

    private bool isTransitioning;
    private Vector3 chapterContentDefaultPosition;
    private RectTransform backButtonRectTransform;
    private Vector2 backButtonDefaultAnchoredPosition;
    private Vector2 modeButtonsDefaultAnchoredPosition;

    private Color defaultBackgroundColor;
    private InputAction backInputAction;
    private InputAction confirmInputAction;

    public bool HasFocus =>
        !isTransitioning &&
        !SceneManager.GetSceneByName(Definitions.StageSelectSceneName).isLoaded;

    private void Awake()
    {
        GameManager.Instance.Sound.PlayBGM(Definitions.SoundType.Lobby);

        backButtonRectTransform = (RectTransform)backButton.transform;
        backButtonDefaultAnchoredPosition = backButtonRectTransform.anchoredPosition;
        modeButtonsDefaultAnchoredPosition = modeButtonsRectTransform.anchoredPosition;

        normalModeButton.onClick.AddListener(() => SelectMode(Definitions.GameMode.Normal));
        hardModeButton.onClick.AddListener(() => SelectMode(Definitions.GameMode.Hard));
        RefreshModeButtons();

        chapterCarousel.Init(this);
        chapterContentDefaultPosition = chapterCarousel.transform.localPosition;
        defaultBackgroundColor = backgroundCamera.backgroundColor;

        var openStageSelectImmediately = OpenStageSelectOnAwake;
        OpenStageSelectOnAwake = false;
        if (openStageSelectImmediately)
        {
            OpenStageSelectImmediately(GameManager.Instance.StageSelection.Chapter);
        }

        backButton.onClick.AddListener(OnBackButton);
        backInputAction = backAction.action.Clone();
        confirmInputAction = confirmAction.action.Clone();
    }

    private void SelectMode(Definitions.GameMode mode)
    {
        if (isTransitioning || mode == GameManager.Instance.StageSelection.Mode)
        {
            return;
        }

        GameManager.Instance.SetMode(mode);
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
        chapterCarousel.Init(this);
        RefreshModeButtons();
    }

    private void RefreshModeButtons()
    {
        var hardModeUnlocked = false;
        modeButtonsRectTransform.gameObject.SetActive(hardModeUnlocked);
        if (!hardModeUnlocked)
        {
            return;
        }

        var currentMode = GameManager.Instance.StageSelection.Mode;
        normalModeButton.interactable = currentMode != Definitions.GameMode.Normal;
        hardModeButton.interactable = currentMode != Definitions.GameMode.Hard;
        normalModeButton.GetComponentInChildren<TMP_Text>().color =
            currentMode == Definitions.GameMode.Normal ? SelectedModeColor : Color.white;
        hardModeButton.GetComponentInChildren<TMP_Text>().color =
            currentMode == Definitions.GameMode.Hard ? SelectedModeColor : Color.white;
    }

    private void OnEnable()
    {
        backInputAction.performed += OnBackAction;
        backInputAction.Enable();

        confirmInputAction.performed += OnConfirmAction;
        confirmInputAction.Enable();
    }

    private void OnDisable()
    {
        backInputAction.performed -= OnBackAction;
        backInputAction.Disable();

        confirmInputAction.performed -= OnConfirmAction;
        confirmInputAction.Disable();
    }

    private void OnBackAction(InputAction.CallbackContext context)
    {
        if (SceneManager.GetSceneByName(Definitions.StageSelectSceneName).isLoaded)
        {
            return;
        }

        OnBackButton();
    }

    private void OnConfirmAction(InputAction.CallbackContext context)
    {
        if (SceneManager.GetSceneByName(Definitions.StageSelectSceneName).isLoaded)
        {
            return;
        }

        chapterCarousel.ConfirmSelectedChapter();
    }

    private void OnDestroy()
    {
        modeButtonsRectTransform.DOKill();
        backInputAction.Dispose();
        confirmInputAction.Dispose();
    }

    public void SelectChapter(int chapter)
    {
        if (isTransitioning)
        {
            return;
        }

        isTransitioning = true;
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
        GameManager.Instance.SetChapter(chapter);

        var stageSelectScene = SceneManager.GetSceneByName(Definitions.StageSelectSceneName);
        if (stageSelectScene.isLoaded)
        {
            FindFirstObjectByType<UI_StageSelect>().KillTransitionTweens();
            var unloadOperation = SceneManager.UnloadSceneAsync(stageSelectScene);
            unloadOperation.completed += _ => LoadStageSelectScene();
            return;
        }

        LoadStageSelectScene();
    }

    public AsyncOperation OpenStageSelectImmediately(int chapter)
    {
        isTransitioning = true;
        GameManager.Instance.SetChapter(chapter);

        chapterCarousel.transform.localPosition = chapterContentDefaultPosition + Vector3.up * GetChapterExitDistance();
        backButtonRectTransform.anchoredPosition = backButtonDefaultAnchoredPosition + Vector2.up * GetCanvasExitDistance();
        modeButtonsRectTransform.anchoredPosition = modeButtonsDefaultAnchoredPosition + Vector2.up * GetCanvasExitDistance();
        backgroundCamera.backgroundColor = GameManager.Instance.GetChapterData(chapter).BackgroundColor;

        UI_StageSelect.PlayIntroOnAwake = false;
        var loadOperation = SceneManager.LoadSceneAsync(Definitions.StageSelectSceneName, LoadSceneMode.Additive);
        loadOperation.completed += _ => isTransitioning = false;
        return loadOperation;
    }

    private void LoadStageSelectScene()
    {
        UI_StageSelect.PlayIntroOnAwake = true;
        var loadOperation = SceneManager.LoadSceneAsync(Definitions.StageSelectSceneName, LoadSceneMode.Additive);
        loadOperation.completed += _ => PlayStageSelectTransition();
    }

    private void OnBackButton()
    {
        if (isTransitioning)
        {
            return;
        }

        isTransitioning = true;
        SceneManager.LoadScene(Definitions.LobbySceneName);
    }

    private void PlayStageSelectTransition()
    {
        var stageSelect = FindFirstObjectByType<UI_StageSelect>();
        var chapterContent = chapterCarousel.transform;
        chapterContent.DOKill();
        backButtonRectTransform.DOKill();
        modeButtonsRectTransform.DOKill();
        backgroundCamera.DOKill();
        chapterContent.localPosition = chapterContentDefaultPosition;
        backButtonRectTransform.anchoredPosition = backButtonDefaultAnchoredPosition;
        modeButtonsRectTransform.anchoredPosition = modeButtonsDefaultAnchoredPosition;
        var chapterColor = GameManager.Instance.GetChapterData(GameManager.Instance.StageSelection.Chapter).BackgroundColor;
        var sequence = DOTween.Sequence();
        sequence.Join(chapterContent.DOLocalMoveY(chapterContentDefaultPosition.y + GetChapterExitDistance(), TransitionDuration).SetEase(Ease.InOutCubic));
        sequence.Join(backButtonRectTransform.DOAnchorPos(backButtonDefaultAnchoredPosition + Vector2.up * GetCanvasExitDistance(), TransitionDuration).SetEase(Ease.InOutCubic));
        sequence.Join(modeButtonsRectTransform.DOAnchorPos(modeButtonsDefaultAnchoredPosition + Vector2.up * GetCanvasExitDistance(), TransitionDuration).SetEase(Ease.InOutCubic));
        sequence.Join(backgroundCamera.DOColor(chapterColor, TransitionDuration).SetEase(Ease.InOutCubic));
        sequence.Join(stageSelect.PlayIntroTransition(TransitionDuration));
        sequence.OnComplete(() => isTransitioning = false);
    }

    public Tween PlayReturnTransition(float duration)
    {
        isTransitioning = true;
        var chapterContent = chapterCarousel.transform;
        chapterContent.DOKill();
        backButtonRectTransform.DOKill();
        modeButtonsRectTransform.DOKill();
        backgroundCamera.DOKill();
        chapterContent.localPosition = chapterContentDefaultPosition + Vector3.up * GetChapterExitDistance();
        backButtonRectTransform.anchoredPosition = backButtonDefaultAnchoredPosition + Vector2.up * GetCanvasExitDistance();
        modeButtonsRectTransform.anchoredPosition = modeButtonsDefaultAnchoredPosition + Vector2.up * GetCanvasExitDistance();
        var sequence = DOTween.Sequence();
        sequence.Join(chapterContent.DOLocalMove(chapterContentDefaultPosition, duration).SetEase(Ease.InOutCubic));
        sequence.Join(backButtonRectTransform.DOAnchorPos(backButtonDefaultAnchoredPosition, duration).SetEase(Ease.InOutCubic));
        sequence.Join(modeButtonsRectTransform.DOAnchorPos(modeButtonsDefaultAnchoredPosition, duration).SetEase(Ease.InOutCubic));
        sequence.Join(backgroundCamera.DOColor(defaultBackgroundColor, duration).SetEase(Ease.InOutCubic));
        sequence.OnComplete(() => isTransitioning = false);
        return sequence;
    }

    private float GetChapterExitDistance()
    {
        return chapterExitDistance > 0f ? chapterExitDistance : ChapterExitDistance;
    }

    private float GetCanvasExitDistance()
    {
        var canvasRectTransform = (RectTransform)backButtonRectTransform.parent;
        return canvasRectTransform.rect.height > 0f ? canvasRectTransform.rect.height : Screen.height;
    }
}
