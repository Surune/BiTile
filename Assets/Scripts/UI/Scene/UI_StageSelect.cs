using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_StageSelect : UI_Popup
{
    enum GameObjects
    {
        UI_Stage_Container,
        Title,
    }
    
    enum Buttons
    {
        BackButton
    }

    [SerializeField]
    public Sprite clearedImage;

    public override void Init()
    {
        base.Init();

        Bind<GameObject>(typeof(GameObjects));
        BindButton(typeof(Buttons));

        GameObject gridPanel = Get<GameObject>((int)GameObjects.UI_Stage_Container);
        foreach (Transform child in gridPanel.transform)
            Managers.Resource.Destroy(child.gameObject);

        int worldNum = Managers.UI.justClickedWorld;
        GetGameObject((int)GameObjects.Title).GetComponentInChildren<TextMeshProUGUI>().text = $"World {worldNum}";
        
        int clearedStage = PlayerPrefs.GetInt("STAGE", 1);
        Debug.Log($"cleared stage : {clearedStage}");
        for (int i = 0; i < 35; i++)
        {
            GameObject stage = Managers.UI.MakeStage<UI_World_Stage>(gridPanel.transform).gameObject;
            stage.name = $"UI_World_Stage{i+1}";
            int stageNum = (worldNum - 1) * 35 + i + 1;
            stage.GetOrAddComponent<UI_World_Stage>().Setinfo($"{stageNum}");
            if (stageNum <= clearedStage)
            {
                stage.GetComponentInChildren<Image>().overrideSprite = clearedImage;
                stage.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
            }
        }

        GetButton((int)Buttons.BackButton).gameObject.BindEvent(OnBackClick);
        Managers.UI.SetCanvas(gameObject, true);
    }

    void OnBackClick(PointerEventData eventData)
    {
        Managers.UI.ClosePopupUI();
    }
}
