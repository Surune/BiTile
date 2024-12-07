using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUI : MonoBehaviour
{
    void Start()
    {
        if (gameObject.scene.name == "LobbyScene")
        {
            Managers.UI.ShowSceneUI<UI_LobbyScreen>();
        }
        else if (gameObject.scene.name == "GameScene")
        {
            Managers.UI.ShowSceneUI<UI_Main>();
        }
        else
        {
            
        }
    }

}
