using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_LobbyScreen : MonoBehaviour
{
    private const float TransitionDuration = 0.4f;

    public static bool OpenStageSelectOnAwake { get; set; }

    [SerializeField] private Button quitButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionButton;

    private RectTransform rootRectTransform;
    private CanvasGroup canvasGroup;
    private readonly List<RectTransform> transitionTargets = new List<RectTransform>();
    private readonly List<Vector2> defaultAnchoredPositions = new List<Vector2>();
    private Sequence transitionSequence;
    private bool isTransitioning;
    private bool openStageSelectImmediatelyOnAwake;

    private void Awake()
    {
        rootRectTransform = (RectTransform)transform;
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        CacheTransitionTargets();

        quitButton.onClick.AddListener(Application.Quit);
        startButton.onClick.AddListener(OnWorldSelect);
        optionButton.onClick.AddListener(OnOptionButton);

        GameManager.Instance.Sound.PlayBGM(Definitions.SoundType.Bgm);

        openStageSelectImmediatelyOnAwake = OpenStageSelectOnAwake;
        OpenStageSelectOnAwake = false;
        if (openStageSelectImmediatelyOnAwake)
        {
            OpenStageSelectImmediately();
        }
    }

    private void OnWorldSelect()
    {
        if (isTransitioning)
        {
            return;
        }

        isTransitioning = true;
        canvasGroup.blocksRaycasts = false;

        UI_ChapterSelect.PlayIntroOnAwake = true;
        var loadOperation = SceneManager.LoadSceneAsync(Definitions.ChapterSelectSceneName, LoadSceneMode.Additive);
        loadOperation.completed += _ => PlayChapterSelectTransition();
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

    private void OpenStageSelectImmediately()
    {
        isTransitioning = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0f;
        SetTransitionPosition(Vector2.up * GetTransitionOffset());
        UI_ChapterSelect.PlayIntroOnAwake = false;
        var loadOperation = SceneManager.LoadSceneAsync(Definitions.ChapterSelectSceneName, LoadSceneMode.Additive);
        loadOperation.completed += _ =>
        {
            var chapterSelect = FindFirstObjectByType<UI_ChapterSelect>();
            chapterSelect.OpenStageSelectImmediately(GameManager.Instance.StageSelection.Chapter);
            openStageSelectImmediatelyOnAwake = false;
        };
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
        return rootRectTransform.rect.height > 0f ? rootRectTransform.rect.height : Screen.height;
    }

    private float GetHorizontalTransitionOffset()
    {
        return rootRectTransform.rect.width > 0f ? rootRectTransform.rect.width : Screen.width;
    }

    private void OnDestroy()
    {
        transitionSequence?.Kill();
    }
}
