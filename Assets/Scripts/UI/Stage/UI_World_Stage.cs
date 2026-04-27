using TMPro;
using UnityEngine;

public class UI_World_Stage : UI_Base
{
    enum GameObjects
    {
        StageNameText
    }

    [SerializeField] private string name;

    public override void Init()
    {
        BindObject(typeof(GameObjects));
        GetComponentInChildren<TextMeshProUGUI>().text = name;
    }

    public void Setinfo(string name)
    {
        this.name = name;
    }
}
