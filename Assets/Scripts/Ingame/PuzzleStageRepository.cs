using UnityEngine;

public class PuzzleStageRepository
{
    private string[] levelInfoRows;

    public PuzzleStageData Load(int stage)
    {
        levelInfoRows ??= Resources.Load<TextAsset>("level_info").text.Split('\n');

        var values = levelInfoRows[stage].Split(',');
        var width = int.Parse(values[2]);
        var height = int.Parse(values[3]);
        var type = values[4];
        var color = values[5];
        var tiles = new TileInfo[width, height];

        for (var row = 0; row < width; row++)
        {
            for (var col = 0; col < height; col++)
            {
                TileInfo info;
                info.Type = type[row * height + col];
                info.Color = color[row * height + col];
                tiles[row, col] = info;
            }
        }

        PuzzleStageData stageData;
        stageData.StageNumber = stage;
        stageData.MaxClicks = int.Parse(values[1]);
        stageData.Width = width;
        stageData.Height = height;
        stageData.Tiles = tiles;
        stageData.HintRow = int.Parse(values[6][2].ToString());
        stageData.HintColumn = int.Parse(values[7][1].ToString());
        return stageData;
    }
}
