using UnityEngine;

public class ColorManager
{
    private ColorPreset colorPreset;

    public void Init()
    {
        colorPreset = Utils.Load<ColorPreset>("ColorPreset");
    }

    public Color GetBackgroundColor(int stage)
    {
        return colorPreset.GetBackgroundColor(GetIndex(stage));
    }

    public Color GetTileColor(int stage)
    {
        return colorPreset.GetTileColor(GetIndex(stage));
    }

    private int GetIndex(int stage)
    {
        return (stage - 1) / 35;
    }
}
