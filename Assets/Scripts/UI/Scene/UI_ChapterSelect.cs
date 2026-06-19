using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_ChapterSelect : MonoBehaviour
{
    [SerializeField] private UI_ChapterCarousel chapterCarousel;

    private void Awake()
    {
        chapterCarousel.Init(this);
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

    private void LoadStageSelectScene()
    {
        SceneManager.LoadScene(Definitions.StageSelectSceneName, LoadSceneMode.Additive);
    }
}
