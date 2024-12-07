using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSelectScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.WorldSelectScene;
        
        Managers.UI.ShowSceneUI<UI_WorldSelect>();
    }

    public override void Clear()
    {
        Debug.Log("Select Scene Clear");
    }
}
