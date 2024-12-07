using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.GameScene;

        Managers.UI.ShowSceneUI<UI_Main>();
    }

    public override void Clear()
    {
        Debug.Log("Game Scene Clear");
    }
}
