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
        var chapterDataList = mode == Definitions.GameMode.Normal
            ? normalChapterDataList
            : hardChapterDataList;
        return chapterDataList.Data[GetIndex(chapter)];
    }

    private int GetIndex(int chapter)
    {
        return chapter - 1;
    }
}
