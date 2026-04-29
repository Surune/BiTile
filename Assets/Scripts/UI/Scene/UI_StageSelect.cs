using UnityEngine;

public class UI_StageSelect : MonoBehaviour
{
    [SerializeField] private Transform stageContainer;
    [SerializeField] private UI_World_Stage stagePrefab;

    private void Awake()
    {
        var clearedStage = PlayerPrefs.GetInt("STAGE", 1);
        for (var i = 1; i < 200; i++)
        {
            var stage = Instantiate(stagePrefab, stageContainer).GetComponent<UI_World_Stage>();
            stage.SetInfo(i, clearedStage);
        }
    }
}
