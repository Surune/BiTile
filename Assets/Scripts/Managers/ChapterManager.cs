public class ChapterManager
{
    private ChapterDataList chapterDataList;

    public ChapterManager(ChapterDataList preset)
    {
        chapterDataList = preset;
    }
    
    public ChapterData GetData(int chapter)
    {
        return chapterDataList.Data[GetIndex(chapter)];
    }

    private int GetIndex(int chapter)
    {
        return chapter - 1;
    }
}
