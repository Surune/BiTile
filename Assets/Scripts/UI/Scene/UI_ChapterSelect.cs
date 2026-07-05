using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_ChapterSelect : MonoBehaviour
{
    private const float TransitionDuration = 0.4f;
    private const float ChapterExitDistance = 7f;

    public static bool PlayIntroOnAwake { get; set; }

    [SerializeField] private UI_ChapterCarousel chapterCarousel;
    [SerializeField] private Camera backgroundCamera;
    [SerializeField] private float chapterExitDistance = ChapterExitDistance;
    [SerializeField] private Button backButton;
    [SerializeField] private InputActionReference backAction;
    [SerializeField] private InputActionReference confirmAction;

    private bool isTransitioning;
    private Vector3 chapterContentDefaultPosition;
    private RectTransform backButtonRectTransform;
    private Vector2 backButtonDefaultAnchoredPosition;
    private Color defaultBackgroundColor;
    private InputAction backInputAction;
    private InputAction confirmInputAction;

    private void Awake()
    {
        chapterCarousel.Init(this);
        chapterContentDefaultPosition = chapterCarousel.transform.localPosition;
        backButtonRectTransform = (RectTransform)backButton.transform;
        backButtonDefaultAnchoredPosition = backButtonRectTransform.anchoredPosition;
        defaultBackgroundColor = backgroundCamera.backgroundColor;

        var shouldPlayIntro = PlayIntroOnAwake;
        PlayIntroOnAwake = false;
        if (shouldPlayIntro)
        {
            PrepareIntroPosition();
        }

        backButton.onClick.AddListener(OnBackButton);
        backInputAction = backAction.action.Clone();
        confirmInputAction = confirmAction.action.Clone();
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
        GameManager.Instance.SelectChapter(chapter);

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

    public void OpenStageSelectImmediately(int chapter)
    {
        isTransitioning = true;
        GameManager.Instance.SelectChapter(chapter);

        chapterCarousel.transform.localPosition = chapterContentDefaultPosition + Vector3.up * GetChapterExitDistance();
        backButtonRectTransform.anchoredPosition = backButtonDefaultAnchoredPosition + Vector2.up * GetCanvasExitDistance();
        backgroundCamera.backgroundColor = GameManager.Instance.Chapter.GetData(chapter).BackgroundColor;

        UI_StageSelect.PlayIntroOnAwake = false;
        var loadOperation = SceneManager.LoadSceneAsync(Definitions.StageSelectSceneName, LoadSceneMode.Additive);
        loadOperation.completed += _ => isTransitioning = false;
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
        var lobbyScene = SceneManager.GetSceneByName(Definitions.LobbySceneName);
        if (!lobbyScene.isLoaded)
        {
            SceneManager.LoadScene(Definitions.LobbySceneName);
            return;
        }

        var lobbyScreen = FindFirstObjectByType<UI_LobbyScreen>();
        var sequence = DOTween.Sequence();
        sequence.Join(PlayExitTransition(TransitionDuration));
        sequence.Join(lobbyScreen.PlayReturnTransition(TransitionDuration));
        sequence.OnComplete(() => SceneManager.UnloadSceneAsync(Definitions.ChapterSelectSceneName));
    }

    private void PlayStageSelectTransition()
    {
        var stageSelect = FindFirstObjectByType<UI_StageSelect>();
        var chapterContent = chapterCarousel.transform;
        chapterContent.DOKill();
        backButtonRectTransform.DOKill();
        backgroundCamera.DOKill();
        chapterContent.localPosition = chapterContentDefaultPosition;
        backButtonRectTransform.anchoredPosition = backButtonDefaultAnchoredPosition;
        var chapterColor = GameManager.Instance.Chapter.GetData(GameManager.Instance.StageSelection.Chapter).BackgroundColor;
        var sequence = DOTween.Sequence();
        sequence.Join(chapterContent.DOLocalMoveY(chapterContentDefaultPosition.y + GetChapterExitDistance(), TransitionDuration).SetEase(Ease.InOutCubic));
        sequence.Join(backButtonRectTransform.DOAnchorPos(backButtonDefaultAnchoredPosition + Vector2.up * GetCanvasExitDistance(), TransitionDuration).SetEase(Ease.InOutCubic));
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
        backgroundCamera.DOKill();
        chapterContent.localPosition = chapterContentDefaultPosition + Vector3.up * GetChapterExitDistance();
        backButtonRectTransform.anchoredPosition = backButtonDefaultAnchoredPosition + Vector2.up * GetCanvasExitDistance();
        var sequence = DOTween.Sequence();
        sequence.Join(chapterContent.DOLocalMove(chapterContentDefaultPosition, duration).SetEase(Ease.InOutCubic));
        sequence.Join(backButtonRectTransform.DOAnchorPos(backButtonDefaultAnchoredPosition, duration).SetEase(Ease.InOutCubic));
        sequence.Join(backgroundCamera.DOColor(defaultBackgroundColor, duration).SetEase(Ease.InOutCubic));
        sequence.OnComplete(() => isTransitioning = false);
        return sequence;
    }

    public Tween PlayIntroTransition(float duration)
    {
        isTransitioning = true;
        var chapterContent = chapterCarousel.transform;
        chapterContent.DOKill();
        backButtonRectTransform.DOKill();
        var sequence = DOTween.Sequence();
        sequence.Join(chapterContent.DOLocalMove(chapterContentDefaultPosition, duration).SetEase(Ease.InOutCubic));
        sequence.Join(backButtonRectTransform.DOAnchorPos(backButtonDefaultAnchoredPosition, duration).SetEase(Ease.InOutCubic));
        sequence.OnComplete(() => isTransitioning = false);
        return sequence;
    }

    private Tween PlayExitTransition(float duration)
    {
        var chapterContent = chapterCarousel.transform;
        chapterContent.DOKill();
        backButtonRectTransform.DOKill();
        backgroundCamera.DOKill();
        var sequence = DOTween.Sequence();
        sequence.Join(chapterContent.DOLocalMove(chapterContentDefaultPosition + Vector3.down * GetChapterExitDistance(), duration).SetEase(Ease.InOutCubic));
        sequence.Join(backButtonRectTransform.DOAnchorPos(backButtonDefaultAnchoredPosition + Vector2.down * GetCanvasExitDistance(), duration).SetEase(Ease.InOutCubic));
        return sequence;
    }

    private void PrepareIntroPosition()
    {
        chapterCarousel.transform.localPosition = chapterContentDefaultPosition + Vector3.down * GetChapterExitDistance();
        backButtonRectTransform.anchoredPosition = backButtonDefaultAnchoredPosition + Vector2.down * GetCanvasExitDistance();
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
