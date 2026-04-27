using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorPreset", menuName = "ScriptableObjects/ColorPreset", order = 3)]
public class ColorPreset : ScriptableObject
{
    [SerializeField, TableView] private List<StageColorData> colors;

    public Color GetBackgroundColor(int index)
    {
        return colors[index].backgroundColor;
    }

    public Color GetTileColor(int index)
    {
        return colors[index].tileColor;
    }
}

[Serializable]
public struct StageColorData
{
    public Color backgroundColor;
    public Color tileColor;
}
