using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.TutorialScene;
        
        Managers.UI.ShowSceneUI<UI_Tutorial>();
    }

    public override void Clear()
    {
        Debug.Log("Select Scene Clear");
    }
}