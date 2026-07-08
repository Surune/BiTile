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

    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private TMP_Text versionText;
    [SerializeField] private InputActionReference confirmAction;

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
        CacheTransitionTargets();

        quitButton.onClick.AddListener(Application.Quit);
        startButton.onClick.AddListener(OnWorldSelect);
        optionButton.onClick.AddListener(OnOptionButton);

        versionText.text = $"v{Application.version} ({BuildInfo.GitHash})";

        confirmInputAction = confirmAction.action.Clone();

        GameManager.Instance.Sound.PlayBGM(Definitions.SoundType.Bgm);

        openStageSelectImmediatelyOnAwake = OpenStageSelectOnAwake;
        OpenStageSelectOnAwake = false;
        if (openStageSelectImmediatelyOnAwake)
        {
            OpenStageSelectImmediately();
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

        OnWorldSelect();
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
