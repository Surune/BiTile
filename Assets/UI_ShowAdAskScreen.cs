using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ShowAdAskScreen : UI_Popup
{
    enum Buttons
    {
        YesButton,
        NoButton,
    }

    public override void Init()
    {
        base.Init();

        BindButton(typeof(Buttons));

        
        GetButton((int)Buttons.YesButton).gameObject.BindEvent(OnYes);
        GetButton((int)Buttons.NoButton).gameObject.BindEvent(OnNo);
    }

    void OnYes(PointerEventData eventData)
    {
        Managers.UI.ClosePopupUI();

        FindObjectOfType<AdmobManager>().ShowRewardedAd();
    }

    void OnNo(PointerEventData eventData)
    {
        Managers.UI.ClosePopupUI();
    }
}
