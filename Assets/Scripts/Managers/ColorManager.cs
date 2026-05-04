using UnityEngine;

public class ColorManager
{
    public Color GetBackgroundColor(int stage)
    {
        return Colorset.backgroundColors[GetIndex(stage)];
    }

    public Color GetTileColor(int stage)
    {
        return Colorset.tileColors[GetIndex(stage)];
    }

    private int GetIndex(int stage)
    {
        return (stage - 1) / 35;
    }
}
