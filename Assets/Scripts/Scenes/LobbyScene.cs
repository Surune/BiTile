using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.LobbyScene;
        
        Managers.UI.ShowSceneUI<UI_LobbyScreen>();
    }

    public override void Clear()
    {
        Debug.Log("Lobby Scene Clear");
    }
}
