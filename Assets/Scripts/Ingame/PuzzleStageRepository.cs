using System.Collections.Generic;

public class PuzzleStageRepository
{
    private static Dictionary<int, Dictionary<string, object>> levelInfoByStage;

    public PuzzleStageData Load(int stage)
    {
        levelInfoByStage ??= LoadLevelInfo();

        var stageRow = levelInfoByStage[stage];
        var width = (int)stageRow["ROW"];
        var height = (int)stageRow["COLUMN"];
        var type = stageRow["TYPE"].ToString();
        var color = stageRow["COLOR"].ToString();
        var hintValues = stageRow["HINT"].ToString().Trim('(', ')').Split(", ");
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
        stageData.MaxClicks = (int)stageRow["LIMIT"];
        stageData.TutorialLkey = stageRow["LKEY"].ToString();
        stageData.Width = width;
        stageData.Height = height;
        stageData.Tiles = tiles;
        stageData.HintRow = int.Parse(hintValues[0]);
        stageData.HintColumn = int.Parse(hintValues[1]);
        return stageData;
    }

    private static Dictionary<int, Dictionary<string, object>> LoadLevelInfo()
    {
        var rows = CSVReader.Read("level_info");
        var levelInfo = new Dictionary<int, Dictionary<string, object>>(rows.Count);

        foreach (var row in rows)
        {
            levelInfo[(int)row["STAGE"]] = row;
        }

        return levelInfo;
    }
}
