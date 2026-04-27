using UnityEngine;

public class ColorManager
{
    private ColorPreset colorPreset;

    public void Init(ColorPreset preset)
    {
        colorPreset = preset;
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
