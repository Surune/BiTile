using UnityEngine;

public class UI_StageSelect : MonoBehaviour
{
    private const int FirstStage = 1;

    [SerializeField] private Transform stageContainer;
    [SerializeField] private UI_World_Stage stagePrefab;
    private int selectedChapter;

#if UNITY_EDITOR
    private string editorLastUnlockedStageText;
#endif

    private void Awake()
    {
        selectedChapter = GameManager.Instance.StageSelection.Chapter;
        RefreshStages(selectedChapter);

#if UNITY_EDITOR
        editorLastUnlockedStageText = SaveManager.LastUnlockedStage.ToString();
#endif
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
            stage.SetInfo(PuzzleStageRepository.GetChapter(i), PuzzleStageRepository.GetStage(i), i, clearedStage);
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
