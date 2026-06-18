using UnityEngine;

public class ColorManager
{
    private ColorPreset colorPreset;

    public void Init(ColorPreset preset)
    {
        colorPreset = preset;
    }

    public Color GetBackgroundColor(int chapter)
    {
        return colorPreset.GetBackgroundColor(GetIndex(chapter));
    }

    public Color GetTileColor(int chapter)
    {
        return colorPreset.GetTileColor(GetIndex(chapter));
    }

    private int GetIndex(int chapter)
    {
        return chapter - 1;
    }
}
