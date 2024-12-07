using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UI_Stage_Container : UI_Base
{
    enum Buttons
    {
        UI_World_Stage1,
        UI_World_Stage2,
        UI_World_Stage3,
        UI_World_Stage4,
        UI_World_Stage5,
        UI_World_Stage6,
        UI_World_Stage7,
        UI_World_Stage8,
        UI_World_Stage9,
        UI_World_Stage10,
        UI_World_Stage11,
        UI_World_Stage12,
        UI_World_Stage13,
        UI_World_Stage14,
        UI_World_Stage15,
        UI_World_Stage16,
        UI_World_Stage17,
        UI_World_Stage18,
        UI_World_Stage19,
        UI_World_Stage20,
        UI_World_Stage21,
        UI_World_Stage22,
        UI_World_Stage23,
        UI_World_Stage24,
        UI_World_Stage25,
        UI_World_Stage26,
        UI_World_Stage27,
        UI_World_Stage28,
        UI_World_Stage29,
        UI_World_Stage30,
        UI_World_Stage31,
        UI_World_Stage32,
        UI_World_Stage33,
        UI_World_Stage34,
        UI_World_Stage35,
    }
    
    public override void Init()
    {
        BindButton(typeof(Buttons));
        for (int i = 0; i < 35; i++)
        {
            GetButton(i).gameObject.BindEvent(OnButtonClicked);
        }
    }

    void OnButtonClicked(PointerEventData eventData)
    {
        int loadStageNum = int.Parse(eventData.pointerClick.GetComponent<UI_World_Stage>()._name);
        Debug.Log($"{loadStageNum}");

        int recentlyCleared = PlayerPrefs.GetInt("STAGE", 1);
        if (recentlyCleared < loadStageNum)
        {
            Managers.Sound.Play("decline");
            Debug.Log($"Your final cleared Stage is Stage {recentlyCleared}");
        }
        else
        {
            Managers.UI.loadStageNum = loadStageNum;
            Managers.Scene.LoadScene(Define.Scene.GameScene);
        }
    }
}
