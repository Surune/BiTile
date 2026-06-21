using System.Collections.Generic;
using UnityEngine;

public class UI_ChapterCarousel : MonoBehaviour
{
    private const int FirstChapter = 1;
    private const float SelectedChapterScale = 1f;
    private const float UnselectedChapterScale = 0.25f;
    private const float MaxVisibleOffset = 3f;

    [SerializeField] private UI_World_Chapter chapterPrefab;
    [SerializeField] private float selectedChapterGap = 4f;
    [SerializeField] private float chapterSpacing = 1.8f;
    [SerializeField] private float carouselLerpSpeed = 12f;
    [SerializeField] private float mouseWheelSensitivity = 1f;

    private readonly List<UI_World_Chapter> chapterViews = new List<UI_World_Chapter>();
    private UI_ChapterSelect chapterSelect;
    private int selectedChapter = FirstChapter;

    public void Init(UI_ChapterSelect chapterSelect)
    {
        this.chapterSelect = chapterSelect;
        RefreshChapters();
    }

    private void Update()
    {
        if (Input.mouseScrollDelta.y >= mouseWheelSensitivity)
        {
            MoveCarousel(-1);
        }
        else if (Input.mouseScrollDelta.y <= -mouseWheelSensitivity)
        {
            MoveCarousel(1);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MoveCarousel(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveCarousel(1);
        }

        RefreshCarousel();
    }

    public void ClickChapter(int chapter, bool isUnlocked)
    {
        if (chapter != selectedChapter)
        {
            FocusChapter(chapter);
            return;
        }

        if (isUnlocked)
        {
            chapterSelect.SelectChapter(chapter);
            return;
        }

        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Decline);
    }

    private void RefreshChapters()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        chapterViews.Clear();
        selectedChapter = Mathf.Clamp(GameManager.Instance.StageSelection.Chapter, FirstChapter, PuzzleStageRepository.TotalChapterCount);

        var clearedStage = SaveManager.LastUnlockedStage;
        for (var chapter = FirstChapter; chapter <= PuzzleStageRepository.TotalChapterCount; chapter++)
        {
            var isUnlocked = PuzzleStageRepository.GetFirstProgressStage(chapter) <= clearedStage;
            var chapterView = Instantiate(chapterPrefab, transform);
            chapterView.Init(this, chapter, isUnlocked);
            chapterViews.Add(chapterView);
        }

        RefreshCarouselImmediate();
    }

    private void MoveCarousel(int direction)
    {
        var nextChapter = Mathf.Clamp(selectedChapter + direction, FirstChapter, PuzzleStageRepository.TotalChapterCount);
        FocusChapter(nextChapter);
    }

    private void FocusChapter(int chapter)
    {
        selectedChapter = chapter;
    }

    private void RefreshCarouselImmediate()
    {
        for (var i = 0; i < chapterViews.Count; i++)
        {
            ApplyCarouselTransform(chapterViews[i], GetLinearOffset(chapterViews[i].Chapter), 1f);
        }
    }

    private void RefreshCarousel()
    {
        var t = Time.deltaTime * carouselLerpSpeed;
        for (var i = 0; i < chapterViews.Count; i++)
        {
            ApplyCarouselTransform(chapterViews[i], GetLinearOffset(chapterViews[i].Chapter), t);
        }
    }

    private int GetLinearOffset(int chapter)
    {
        return chapter - selectedChapter;
    }

    private void ApplyCarouselTransform(UI_World_Chapter chapterView, int offset, float t)
    {
        var clampedOffset = Mathf.Clamp(offset, -MaxVisibleOffset, MaxVisibleOffset);
        chapterView.SetSelected(offset == 0);
        chapterView.gameObject.SetActive(Mathf.Abs(offset) <= MaxVisibleOffset);

        var distance = Mathf.Abs(clampedOffset);
        var targetX = GetCarouselX(clampedOffset);
        var targetPosition = Vector3.right * targetX;
        var targetScale = Vector3.one * (offset == 0 ? SelectedChapterScale : UnselectedChapterScale);

        chapterView.transform.localPosition = Vector3.Lerp(chapterView.transform.localPosition, targetPosition, t);
        chapterView.transform.localScale = Vector3.Lerp(chapterView.transform.localScale, targetScale, t);
        chapterView.transform.SetSiblingIndex(Mathf.RoundToInt(MaxVisibleOffset - distance));
    }

    private float GetCarouselX(float offset)
    {
        if (offset == 0f)
        {
            return 0f;
        }

        var direction = Mathf.Sign(offset);
        var distanceFromSelected = selectedChapterGap + (Mathf.Abs(offset) - 1f) * chapterSpacing;
        return direction * distanceFromSelected;
    }
}
