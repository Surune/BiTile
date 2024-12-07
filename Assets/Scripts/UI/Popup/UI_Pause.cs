using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Pause : UI_Popup
{
    enum GameObjects
    {
        UI_PausePanel
    }

    enum Buttons
    {
        MainMenuButton,
        TempButton,
        ClosePopupButton
    }
    
    public override void Init()
    {
        base.Init();

        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));
        
        GetButton((int)Buttons.ClosePopupButton).gameObject.BindEvent(OnClosePopup);
        GetButton((int)Buttons.MainMenuButton).gameObject.BindEvent(OnGoMainMenu);
    }

    void OnClosePopup(PointerEventData eventData)
    {
        base.ClosePopupUI();
    }
    
    void OnGoMainMenu(PointerEventData eventData)
    {
        Managers.Scene.LoadScene(Define.Scene.LobbyScene);
    }
}
