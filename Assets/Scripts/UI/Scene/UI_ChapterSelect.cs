using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_ChapterSelect : MonoBehaviour
{
    private const int FirstChapter = 1;

    [SerializeField] private Transform chapterContainer;
    [SerializeField] private UI_World_Chapter chapterPrefab;

    private void Awake()
    {
        RefreshChapters();
    }

    public void SelectChapter(int chapter)
    {
        GameManager.Instance.SelectChapter(chapter);

        var stageSelectScene = SceneManager.GetSceneByName(Definitions.StageSelectSceneName);
        if (stageSelectScene.isLoaded)
        {
            var unloadOperation = SceneManager.UnloadSceneAsync(stageSelectScene);
            unloadOperation.completed += _ => LoadStageSelectScene();
            return;
        }

        LoadStageSelectScene();
    }

    private void RefreshChapters()
    {
        foreach (Transform child in chapterContainer)
        {
            Destroy(child.gameObject);
        }

        var clearedStage = SaveManager.LastUnlockedStage;
        for (var chapter = FirstChapter; chapter <= PuzzleStageRepository.TotalChapterCount; chapter++)
        {
            var isUnlocked = PuzzleStageRepository.GetFirstProgressStage(chapter) <= clearedStage;
            var chapterView = Instantiate(chapterPrefab, chapterContainer);
            chapterView.Init(this, chapter, isUnlocked);
        }
    }

    private void LoadStageSelectScene()
    {
        SceneManager.LoadScene(Definitions.StageSelectSceneName, LoadSceneMode.Additive);
    }
}
