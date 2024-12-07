using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_World_Stage : UI_Base
{
    enum GameObjects
    {
        StageNameText
    }

    public string _name;

    public override void Init()
    {
        BindObject(typeof(GameObjects));
        GetComponentInChildren<TextMeshProUGUI>().text = _name;
    }

    public void Setinfo(string name)
    {
        _name = name;
    }
}
