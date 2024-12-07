using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_LobbyScreen : UI_Scene
{
    enum Texts
    {
        TitleText
    }
    
    enum GameObjects
    {
        LobbyPanel,
        TitleIcon
    }

    enum Buttons
    {
        StartButton,
        PrivacyPolicyButton,
    }

    public override void Init()
    {
        base.Init();

        BindButton(typeof(Buttons));
        //BindText(typeof(Texts));
        BindObject(typeof(GameObjects));

        GetButton((int)Buttons.StartButton).gameObject.BindEvent(OnWorldSelect);
        GetButton((int)Buttons.PrivacyPolicyButton).gameObject.BindEvent(OnPrivacyPolicy);

        int height = Screen.height - 300;
        GetGameObject((int)GameObjects.TitleIcon).transform.localPosition = new Vector3(0f, height / 6, 0);
        GetGameObject((int)GameObjects.TitleIcon).transform.localPosition = new Vector3(0f, height / 6 - 150, 0);

        Managers.Sound.Play("Music", Define.Sound.Bgm);
    }

    void OnWorldSelect(PointerEventData eventData)
    {
        Managers.Scene.LoadScene(Define.Scene.WorldSelectScene);
    }
    
    void OnPrivacyPolicy(PointerEventData eventData)
    {
        Application.OpenURL("https://sites.google.com/view/bitile-privacy-policy");
    }
}
