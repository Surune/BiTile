using UnityEngine;

public class ChapterManager
{
    private readonly ChapterDataList normalChapterDataList;
    private readonly ChapterDataList hardChapterDataList;

    public ChapterManager(ChapterDataList normalPreset, ChapterDataList hardPreset)
    {
        normalChapterDataList = normalPreset;
        hardChapterDataList = hardPreset;
    }
    
    public ChapterData GetData(Definitions.GameMode mode, int chapter)
    {
        return GetDataList(mode).Data[GetIndex(chapter)];
    }

    public Material GetCompletedMaterial(Definitions.GameMode mode)
    {
        return GetDataList(mode).CompletedMaterial;
    }

    private ChapterDataList GetDataList(Definitions.GameMode mode)
    {
        return mode == Definitions.GameMode.Normal
            ? normalChapterDataList
            : hardChapterDataList;
    }

    private int GetIndex(int chapter)
    {
        return chapter - 1;
    }
}
