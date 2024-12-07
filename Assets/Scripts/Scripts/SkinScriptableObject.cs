using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SkinScriptableObject", order = 1)]
public class SkinScriptableObject : ScriptableObject
{
    public string skinName;
    public Sprite skinImage;
}