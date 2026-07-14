using UnityEngine;

public struct PuzzleStageData
{
    public int Chapter;
    public int Stage;
    public int MaxClicks;
    public int Width;
    public int Height;
    public TileInfo[,] Tiles;
    public Vector2Int HintPosition;
    public bool ShowHint;
    public Definitions.LKey TutorialLkey;
}
