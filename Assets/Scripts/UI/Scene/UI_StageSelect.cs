using UnityEngine;

public class UI_StageSelect : MonoBehaviour
{
    private const int FirstStage = 1;
    private const int LastStage = 200;

    [SerializeField] private Transform stageContainer;
    [SerializeField] private UI_World_Stage stagePrefab;

#if UNITY_EDITOR
    private string editorLastUnlockedStageText;
#endif

    private void Awake()
    {
        RefreshStages();

#if UNITY_EDITOR
        editorLastUnlockedStageText = SaveManager.LastUnlockedStage.ToString();
#endif
    }

    private void RefreshStages()
    {
        foreach (Transform child in stageContainer)
        {
            Destroy(child.gameObject);
        }

        var clearedStage = SaveManager.LastUnlockedStage;
        for (var i = FirstStage; i <= LastStage; i++)
        {
            var stage = Instantiate(stagePrefab, stageContainer).GetComponent<UI_World_Stage>();
            stage.SetInfo(i, clearedStage);
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
        var lastUnlockedStage = Mathf.Clamp(int.Parse(editorLastUnlockedStageText), FirstStage, LastStage);
        editorLastUnlockedStageText = lastUnlockedStage.ToString();
        SaveManager.LastUnlockedStage = lastUnlockedStage;
        RefreshStages();
    }
#endif
}
