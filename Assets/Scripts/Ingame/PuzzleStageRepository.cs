using System;
using System.Collections.Generic;

public class PuzzleStageRepository
{
    private static Dictionary<(int, int), PuzzleStageData> levelInfo;
    private static List<(int Chapter, int Stage)> stageKeys;
    private static int totalChapterCount;

    public static int TotalStageCount
    {
        get
        {
            levelInfo ??= LoadLevelInfo();
            return stageKeys.Count;
        }
    }

    public static int TotalChapterCount
    {
        get
        {
            levelInfo ??= LoadLevelInfo();
            return totalChapterCount;
        }
    }

    public static int GetChapter(int progressStage)
    {
        levelInfo ??= LoadLevelInfo();
        return stageKeys[progressStage - 1].Chapter;
    }

    public static int GetStage(int progressStage)
    {
        levelInfo ??= LoadLevelInfo();
        return stageKeys[progressStage - 1].Stage;
    }

    public static int GetProgressStage(int chapter, int stage)
    {
        levelInfo ??= LoadLevelInfo();
        return stageKeys.IndexOf((chapter, stage)) + 1;
    }

    public static int GetFirstProgressStage(int chapter)
    {
        levelInfo ??= LoadLevelInfo();
        return stageKeys.IndexOf((chapter, 1)) + 1;
    }

    public static int GetStageCount(int chapter)
    {
        levelInfo ??= LoadLevelInfo();
        var count = 0;
        for (var i = 0; i < stageKeys.Count; i++)
        {
            if (stageKeys[i].Chapter == chapter)
            {
                count++;
            }
        }

        return count;
    }

    public PuzzleStageData Load(int chapter, int stage)
    {
        levelInfo ??= LoadLevelInfo();
        return CopyStageData(levelInfo[(chapter, stage)]);
    }

#if UNITY_EDITOR
    public void Reload()
    {
        levelInfo = LoadLevelInfo();
    }
#endif

    private static Dictionary<(int, int), PuzzleStageData> LoadLevelInfo()
    {
        var rows = CSVReader.Read("level_info");
        var levelInfo = new Dictionary<(int, int), PuzzleStageData>(rows.Count);
        var keys = new List<(int Chapter, int Stage)>(rows.Count);
        totalChapterCount = 0;

        foreach (var row in rows)
        {
            var chapter = int.Parse(row["CHAPTER"]);
            var stage = int.Parse(row["STAGE"]);
            if (chapter > totalChapterCount)
            {
                totalChapterCount = chapter;
            }

            PuzzleStageData stageData;
            stageData.Chapter = chapter;
            stageData.Stage = stage;
            stageData.MaxClicks = int.Parse(row["LIMIT"]);
            stageData.Width = int.Parse(row["ROW"]);
            stageData.Height = int.Parse(row["COLUMN"]);
            stageData.Tiles = CreateTileInfo(stageData.Width, stageData.Height, row["TYPE"], row["COLOR"]);
            var hintValues = row["HINT"].Trim('(', ')').Split(", ");
            stageData.HintPosition = default;
            stageData.HintPosition.x = int.Parse(hintValues[0]);
            stageData.HintPosition.y = int.Parse(hintValues[1]);
            stageData.TutorialLkey = row["LKEY"] == string.Empty
                ? Definitions.LKey.None
                : Enum.Parse<Definitions.LKey>(row["LKEY"]);

            var key = (chapter, stage);
            keys.Add(key);
            levelInfo.Add(key, stageData);
        }

        stageKeys = keys;
        return levelInfo;
    }

    private static PuzzleStageData CopyStageData(PuzzleStageData source)
    {
        var stageData = source;
        stageData.Tiles = CopyTileInfo(source.Tiles, source.Width, source.Height);
        return stageData;
    }

    private static TileInfo[,] CopyTileInfo(TileInfo[,] source, int width, int height)
    {
        var tiles = new TileInfo[width, height];
        for (var row = 0; row < width; row++)
        {
            for (var column = 0; column < height; column++)
            {
                tiles[row, column] = source[row, column];
            }
        }

        return tiles;
    }

    private static TileInfo[,] CreateTileInfo(int width, int height, string type, string color)
    {
        var tiles = new TileInfo[width, height];
        for (var row = 0; row < width; row++)
        {
            for (var column = 0; column < height; column++)
            {
                TileInfo info;
                info.Type = type[row * height + column];
                info.Color = color[row * height + column];
                tiles[row, column] = info;
            }
        }

        return tiles;
    }
}
