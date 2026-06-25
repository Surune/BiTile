using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class UI_ChapterSelect : MonoBehaviour
{
    private const float TransitionDuration = 0.4f;
    private const float ChapterExitDistance = 7f;

    [SerializeField] private UI_ChapterCarousel chapterCarousel;
    [SerializeField] private Camera backgroundCamera;
    [SerializeField] private float chapterExitDistance = ChapterExitDistance;

    private bool isTransitioning;
    private Vector3 chapterContentDefaultPosition;
    private Color defaultBackgroundColor;

    private void Awake()
    {
        chapterCarousel.Init(this);
        chapterContentDefaultPosition = chapterCarousel.transform.localPosition;
        defaultBackgroundColor = backgroundCamera.backgroundColor;
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
            FindObjectOfType<UI_StageSelect>()?.KillTransitionTweens();
            var unloadOperation = SceneManager.UnloadSceneAsync(stageSelectScene);
            unloadOperation.completed += _ => LoadStageSelectScene();
            return;
        }

        LoadStageSelectScene();
    }

    private void LoadStageSelectScene()
    {
        UI_StageSelect.PlayIntroOnAwake = true;
        var loadOperation = SceneManager.LoadSceneAsync(Definitions.StageSelectSceneName, LoadSceneMode.Additive);
        loadOperation.completed += _ => PlayStageSelectTransition();
    }

    private void PlayStageSelectTransition()
    {
        var stageSelect = FindObjectOfType<UI_StageSelect>();
        if (stageSelect == null)
        {
            isTransitioning = false;
            return;
        }

        var chapterContent = chapterCarousel.transform;
        chapterContent.DOKill();
        backgroundCamera.DOKill();
        chapterContent.localPosition = chapterContentDefaultPosition;
        var chapterColor = GameManager.Instance.Chapter.GetData(GameManager.Instance.StageSelection.Chapter).BackgroundColor;
        var sequence = DOTween.Sequence();
        sequence.Join(chapterContent.DOLocalMoveY(chapterContentDefaultPosition.y + GetChapterExitDistance(), TransitionDuration).SetEase(Ease.InOutCubic));
        sequence.Join(backgroundCamera.DOColor(chapterColor, TransitionDuration).SetEase(Ease.InOutCubic));
        sequence.Join(stageSelect.PlayIntroTransition(TransitionDuration));
        sequence.OnComplete(() => isTransitioning = false);
    }

    public Tween PlayReturnTransition(float duration)
    {
        isTransitioning = true;
        var chapterContent = chapterCarousel.transform;
        chapterContent.DOKill();
        backgroundCamera.DOKill();
        chapterContent.localPosition = chapterContentDefaultPosition + Vector3.up * GetChapterExitDistance();
        var sequence = DOTween.Sequence();
        sequence.Join(chapterContent.DOLocalMove(chapterContentDefaultPosition, duration).SetEase(Ease.InOutCubic));
        sequence.Join(backgroundCamera.DOColor(defaultBackgroundColor, duration).SetEase(Ease.InOutCubic));
        sequence.OnComplete(() => isTransitioning = false);
        return sequence;
    }

    private float GetChapterExitDistance()
    {
        return chapterExitDistance > 0f ? chapterExitDistance : ChapterExitDistance;
    }
}
