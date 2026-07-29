using System;
using System.Collections.Generic;

public class PuzzleStageRepository
{
    private readonly Definitions.GameMode mode;
    private Dictionary<(int, int), PuzzleStageData> levelInfo;
    private List<(int Chapter, int Stage)> stageKeys;
    private int totalChapterCount;

    public int TotalStageCount
    {
        get
        {
            EnsureLoaded();
            return stageKeys.Count;
        }
    }

    public int TotalChapterCount
    {
        get
        {
            EnsureLoaded();
            return totalChapterCount;
        }
    }

    public PuzzleStageRepository(Definitions.GameMode mode)
    {
        this.mode = mode;
    }

    public int GetChapter(int progressStage)
    {
        EnsureLoaded();
        return stageKeys[progressStage - 1].Chapter;
    }

    public int GetStage(int progressStage)
    {
        EnsureLoaded();
        return stageKeys[progressStage - 1].Stage;
    }

    public int GetProgressStage(int chapter, int stage)
    {
        EnsureLoaded();
        return stageKeys.IndexOf((chapter, stage)) + 1;
    }

    public int GetFirstProgressStage(int chapter)
    {
        EnsureLoaded();
        return stageKeys.IndexOf((chapter, 1)) + 1;
    }

    public int GetStageCount(int chapter)
    {
        EnsureLoaded();
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
        EnsureLoaded();
        return CopyStageData(levelInfo[(chapter, stage)]);
    }

#if UNITY_EDITOR
    public void Reload()
    {
        LoadLevelInfo();
    }
#endif

    private void EnsureLoaded()
    {
        if (levelInfo == null)
        {
            LoadLevelInfo();
        }
    }

    private void LoadLevelInfo()
    {
        var resourceName = mode == Definitions.GameMode.Normal
            ? "level_info"
            : "hard_level_info";
        var rows = CSVReader.Read(resourceName);
        levelInfo = new Dictionary<(int, int), PuzzleStageData>(rows.Count);
        stageKeys = new List<(int Chapter, int Stage)>(rows.Count);
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
            stageData.HintPosition = default;
            stageData.HintPosition.x = int.Parse(row["HINT_ROW"]);
            stageData.HintPosition.y = int.Parse(row["HINT_COLUMN"]);
            stageData.ShowHint = bool.Parse(row["SHOW_HINT"]);
            stageData.TutorialLkey = row["LKEY"] == string.Empty
                ? Definitions.LKey.None
                : Enum.Parse<Definitions.LKey>(row["LKEY"]);

            var key = (chapter, stage);
            stageKeys.Add(key);
            levelInfo.Add(key, stageData);
        }
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
