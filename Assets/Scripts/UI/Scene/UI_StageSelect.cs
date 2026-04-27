using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_StageSelect : UI_Popup
{
    [SerializeField] private Button backButton;
    
    enum GameObjects
    {
        UI_Stage_Container,
        Title,
    }

    [SerializeField]
    public Sprite clearedImage;

    public override void Init()
    {
        base.Init();

        Bind<GameObject>(typeof(GameObjects));

        var gridPanel = Get<GameObject>((int)GameObjects.UI_Stage_Container);
        foreach (Transform child in gridPanel.transform)
            Managers.Resource.Destroy(child.gameObject);

        var worldNum = Managers.UI.justClickedWorld;
        GetGameObject((int)GameObjects.Title).GetComponentInChildren<TextMeshProUGUI>().text = $"World {worldNum}";
        
        var clearedStage = PlayerPrefs.GetInt("STAGE", 1);
        Debug.Log($"cleared stage : {clearedStage}");
        for (var i = 0; i < 35; i++)
        {
            var stage = Managers.UI.MakeStage<UI_World_Stage>(gridPanel.transform).gameObject;
            stage.name = $"UI_World_Stage{i+1}";
            var stageNum = (worldNum - 1) * 35 + i + 1;
            stage.GetOrAddComponent<UI_World_Stage>().Setinfo($"{stageNum}");
            if (stageNum > clearedStage)
            {
                continue;
            }
            stage.GetComponentInChildren<Image>().overrideSprite = clearedImage;
            stage.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        }

        backButton.onClick.AddListener(OnBackClick);
        Managers.UI.SetCanvas(gameObject, true);
    }

    private void OnBackClick()
    {
        Managers.UI.ClosePopupUI();
    }
}
