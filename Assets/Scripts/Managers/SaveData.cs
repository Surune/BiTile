using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int lastUnlockedStage = 1;
    public List<int> starredProgressStages = new List<int>();
    public int normalClearedChapterMask;
    public bool normalModeCleared;
    public int hardChapter1ClearedStageCount;
    public int hardChapter2ClearedStageCount;
    public int hardChapter3ClearedStageCount;
    public int hardChapter4ClearedStageCount;
    public int hardChapter5ClearedStageCount;
    public int hardChapter6ClearedStageCount;
    public int hardChapter7ClearedStageCount;
    public List<int> hardStarredProgressStages = new List<int>();
    public bool hardModeCleared;
}
