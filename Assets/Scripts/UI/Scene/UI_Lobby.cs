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
    private bool isTransitioning;
    private InputAction confirmInputAction;

    private void Awake()
    {
        CacheTransitionTargets();

        quitButton.onClick.AddListener(Application.Quit);
        startButton.onClick.AddListener(OnGameStart);
        optionButton.onClick.AddListener(OnOptionButton);

        versionText.text = $"{Application.version}({BuildInfo.GitHash})";

        confirmInputAction = confirmAction.action.Clone();

        GameManager.Instance.Sound.PlayBGM(Definitions.SoundType.Bgm);
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
            SceneManager.GetSceneByName(Definitions.StageSelectSceneName).isLoaded)
        {
            return;
        }

        OnGameStart();
    }

    private void OnGameStart()
    {
        if (isTransitioning)
        {
            return;
        }

        isTransitioning = true;
        canvasGroup.blocksRaycasts = false;

        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.GameStart);
        SceneManager.LoadScene(Definitions.ChapterSelectSceneName);
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

    private float GetHorizontalTransitionOffset()
    {
        return canvasRectTransform.rect.width > 0f ? canvasRectTransform.rect.width : Screen.width;
    }

    private void OnDestroy()
    {
        transitionSequence?.Kill();
        confirmInputAction.Dispose();
    }
}
