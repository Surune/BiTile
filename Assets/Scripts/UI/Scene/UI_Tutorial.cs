using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Tutorial : UI_Scene
{
    enum Texts
    {
        StageText,
        MoveTurnText
    }

    enum Buttons
    {
        //BindColorButton,
        //ChangeColorButton,
        BGMButton,
        SFXButton,
        SkinButton,
        ResetButton,
        HintButton,
        ExitButton
    }
    
    public override void Init()
    {
        base.Init();

        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitButton);
        GetButton((int)Buttons.BGMButton).gameObject.BindEvent(OnBGMButton);
        GetButton((int)Buttons.SFXButton).gameObject.BindEvent(OnSFXButton);
        
        GetButton((int)Buttons.BGMButton).transform.GetChild(0).gameObject.SetActive(Managers.Sound.bgmOn);
        GetButton((int)Buttons.SFXButton).transform.GetChild(0).gameObject.SetActive(Managers.Sound.sfxOn);
        
    }

    void OnSkinButton(PointerEventData eventData)
    {
        Managers.UI.ShowPopupUI<UI_Popup>("UI_Skin");
    }
    
    void OnBGMButton(PointerEventData eventData)
    {
        Managers.Sound.ToggleBGMMute();
        GetButton((int)Buttons.BGMButton).transform.GetChild(0).gameObject.SetActive(Managers.Sound.bgmOn);
    }
    
    void OnSFXButton(PointerEventData eventData)
    {
        Managers.Sound.ToggleSFXMute();
        GetButton((int)Buttons.SFXButton).transform.GetChild(0).gameObject.SetActive(Managers.Sound.sfxOn);
    }

    void OnExitButton(PointerEventData eventData)
    {
        Managers.Scene.LoadScene(Define.Scene.WorldSelectScene);
        //Managers.UI.ShowPopupUI<UI_ExitAskScreen>();
    }
}