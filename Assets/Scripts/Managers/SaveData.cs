using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public List<ChapterProgressData> chapterProgresses = new List<ChapterProgressData>();
    public bool hardModeUnlocked;
}

[Serializable]
public class ChapterProgressData
{
    public Definitions.GameMode mode;
    public int chapterId;
    public int clearedStageCount;
    public List<int> starredStages = new List<int>();
}
