using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UI_WorldSelect : UI_Scene
{
    enum Texts
    {
        WorldText,
        StageText
    }
    
    enum Buttons
    {
        WorldButton,
        LeftArrowButton,
        RightArrowButton,
    }

    enum GameObjects
    {
        StagePanel
    }

    private const int tutorialWorldNum = 0;
    private int worldNum = 1;
    private int worldMax = 6;

    public override void Init()
    {
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));
        
        GetButton((int)Buttons.WorldButton).gameObject.BindEvent(OnWorld);
        GetButton((int)Buttons.LeftArrowButton).gameObject.BindEvent(OnLeftArrowButton);
        GetButton((int)Buttons.RightArrowButton).gameObject.BindEvent(OnRightArrowButton);

        worldNum = PlayerPrefs.GetInt("WORLD", 0);
        SetContents();
    }
    
    void OnWorld(PointerEventData eventData)
    {
        if (worldNum == tutorialWorldNum)
        {
            Managers.Scene.LoadScene(Define.Scene.TutorialScene);
        }
        else
        {
            Managers.UI.ShowPopupUI<UI_StageSelect>();
            Managers.UI.justClickedWorld = worldNum;
        }
    }

    void SetContents()
    {
        if (worldNum == tutorialWorldNum)
        {
            GetText((int)Texts.WorldText).text = $"Tutorial";
            GetText((int)Texts.WorldText).DOColor(Color.black, 0.5f);
            GetText((int)Texts.StageText).text = $"HOW TO PLAY";
            GetText((int)Texts.StageText).DOColor(Color.white, 0.5f);
            GetButton((int)Buttons.WorldButton).gameObject.GetComponentInChildren<Image>()
                .DOColor(Color.white, 0.5f);
            GetGameObject((int)GameObjects.StagePanel).gameObject.GetComponentInChildren<Image>()
                .DOColor(Color.black, 0.5f);
        }
        else
        {
            GetText((int)Texts.WorldText).text = $"World {worldNum}";
            GetText((int)Texts.WorldText).DOColor(Color.white, 0.5f);
            GetText((int)Texts.StageText).text = $"STAGE {1 + 35 * (worldNum - 1)}~{(35 * worldNum)}";
            GetText((int)Texts.StageText).DOColor(Colorset.tileColors[worldNum - 1], 0.5f);
            GetButton((int)Buttons.WorldButton).gameObject.GetComponentInChildren<Image>()
                .DOColor(Colorset.tileColors[worldNum - 1], 0.5f);
            GetGameObject((int)GameObjects.StagePanel).gameObject.GetComponentInChildren<Image>()
                .DOColor(Colorset.backgroundColors[worldNum - 1], 0.5f);
        }
    }
    
    void OnLeftArrowButton(PointerEventData eventData)
    {
        worldNum--;
        if (worldNum < 0)
        {
            worldNum = worldMax;
        }
        PlayerPrefs.SetInt("WORLD", worldNum);
        SetContents();
    }
    
    void OnRightArrowButton(PointerEventData eventData)
    {
        worldNum++;
        if (worldNum > worldMax)
        {
            worldNum = 0;
        }
        PlayerPrefs.SetInt("WORLD", worldNum);
        SetContents();
    }
}
