using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_StageSelect : MonoBehaviour
{
    private const int FirstStage = 1;
    private const float TransitionDuration = 0.4f;

    public static bool PlayIntroOnAwake { get; set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Transform stageContainer;
    [SerializeField] private UI_World_Stage stagePrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private InputActionReference backAction;

    private int selectedChapter;
    private RectTransform rootRectTransform;
    private readonly List<RectTransform> transitionTargets = new List<RectTransform>();
    private readonly List<Vector2> defaultAnchoredPositions = new List<Vector2>();
    private Sequence transitionSequence;
    private bool isTransitioning;
    private InputAction backInputAction;

#if UNITY_EDITOR
    private string editorLastUnlockedStageText;
#endif

    private void Awake()
    {
        rootRectTransform = (RectTransform)transform;

        CacheTransitionTargets();
        var shouldPlayIntro = PlayIntroOnAwake;
        PlayIntroOnAwake = false;
        if (shouldPlayIntro)
        {
            PrepareIntroPosition();
        }

        backButton.onClick.AddListener(OnBackButton);
        backInputAction = backAction.action.Clone();
        
        selectedChapter = GameManager.Instance.StageSelection.Chapter;
        RefreshStages(selectedChapter);

#if UNITY_EDITOR
        editorLastUnlockedStageText = SaveManager.LastUnlockedStage.ToString();
#endif
    }

    private void OnEnable()
    {
        backInputAction.performed += OnBackAction;
        backInputAction.Enable();
    }

    private void OnDisable()
    {
        backInputAction.performed -= OnBackAction;
        backInputAction.Disable();
    }

    private void OnBackAction(InputAction.CallbackContext context)
    {
        OnBackButton();
    }

    public Tween PlayIntroTransition(float duration)
    {
        KillTransitionTweens();
        isTransitioning = true;
        canvasGroup.blocksRaycasts = false;
        transitionSequence = CreateMoveSequence(Vector2.zero, duration);
        transitionSequence.OnComplete(() =>
        {
            isTransitioning = false;
            canvasGroup.blocksRaycasts = true;
        });
        return transitionSequence;
    }

    public void KillTransitionTweens()
    {
        transitionSequence?.Kill();
        transitionSequence = null;

        for (var i = 0; i < transitionTargets.Count; i++)
        {
            if (transitionTargets[i] != null)
            {
                DOTween.Kill(transitionTargets[i]);
            }
        }
    }

    private void OnBackButton()
    {
        if (isTransitioning)
        {
            return;
        }

        isTransitioning = true;
        canvasGroup.blocksRaycasts = false;

        transitionSequence = CreateMoveSequence(Vector2.down * GetTransitionOffset(), TransitionDuration);

        var chapterSelect = FindFirstObjectByType<UI_ChapterSelect>();
        transitionSequence.Join(chapterSelect.PlayReturnTransition(TransitionDuration));

        transitionSequence.OnComplete(() =>
        {
            transitionSequence = null;
            SceneManager.UnloadSceneAsync(Definitions.StageSelectSceneName);
        });
    }

    private Sequence CreateMoveSequence(Vector2 offset, float duration)
    {
        var sequence = DOTween.Sequence().SetTarget(this).SetLink(gameObject);
        for (var i = 0; i < transitionTargets.Count; i++)
        {
            var target = transitionTargets[i];
            if (target == null)
            {
                continue;
            }

            sequence.Join(target.DOAnchorPos(defaultAnchoredPositions[i] + offset, duration)
                .SetEase(Ease.InOutCubic)
                .SetTarget(target)
                .SetLink(target.gameObject));
        }

        return sequence;
    }

    private void PrepareIntroPosition()
    {
        canvasGroup.blocksRaycasts = false;
        var introOffset = Vector2.down * GetTransitionOffset();
        for (var i = 0; i < transitionTargets.Count; i++)
        {
            transitionTargets[i].anchoredPosition = defaultAnchoredPositions[i] + introOffset;
        }
    }

    private void OnDestroy()
    {
        KillTransitionTweens();
        backInputAction.Dispose();
    }

    private float GetTransitionOffset()
    {
        return rootRectTransform.rect.height > 0f ? rootRectTransform.rect.height : Screen.height;
    }

    private void CacheTransitionTargets()
    {
        transitionTargets.Clear();
        defaultAnchoredPositions.Clear();

        foreach (Transform child in transform)
        {
            if (child is RectTransform rectTarget)
            {
                transitionTargets.Add(rectTarget);
                defaultAnchoredPositions.Add(rectTarget.anchoredPosition);
            }
        }
    }

    private void RefreshStages()
    {
        RefreshStages(selectedChapter);
    }

    private void RefreshStages(int chapter)
    {
        foreach (Transform child in stageContainer)
        {
            Destroy(child.gameObject);
        }

        var clearedStage = SaveManager.LastUnlockedStage;
        for (var i = FirstStage; i <= PuzzleStageRepository.TotalStageCount; i++)
        {
            if (PuzzleStageRepository.GetChapter(i) != chapter)
            {
                continue;
            }

            var stage = Instantiate(stagePrefab, stageContainer);
            var stageChapter = PuzzleStageRepository.GetChapter(i);
            var stageNumber = PuzzleStageRepository.GetStage(i);
            var starColor = GameManager.Instance.Chapter.GetData(stageChapter).TileColor;
            stage.SetInfo(stageChapter, stageNumber, i, clearedStage, SaveManager.HasStar(i), starColor);
        }
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(260f));
        GUILayout.Label("Stage Selection Unlock");
        GUILayout.Label($"Current Last Unlocked: {SaveManager.LastUnlockedStage}");
        editorLastUnlockedStageText = GUILayout.TextField(editorLastUnlockedStageText, GUILayout.Width(120f));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply"))
        {
            ApplyEditorLastUnlockedStage();
        }

        if (GUILayout.Button("Reset"))
        {
            editorLastUnlockedStageText = FirstStage.ToString();
            ApplyEditorLastUnlockedStage();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void ApplyEditorLastUnlockedStage()
    {
        var lastUnlockedStage = Mathf.Clamp(int.Parse(editorLastUnlockedStageText), FirstStage, PuzzleStageRepository.TotalStageCount);
        editorLastUnlockedStageText = lastUnlockedStage.ToString();
        SaveManager.LastUnlockedStage = lastUnlockedStage;
        RefreshStages();
    }
#endif
}
