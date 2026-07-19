public static class Definitions
{
    public static string LobbySceneName => "LobbyScene";
    public static string OptionSceneName => "OptionsScene";
    public static string ChapterSelectSceneName => "ChapterSelectScene";
    public static string StageSelectSceneName => "StageSelectScene";
    public static string GameSceneName => "MainGameScene";

    public enum SoundType
    {
        None = 0,
        Bgm = 1,
        Music = 2,
        Decline = 3,
        GameStart = 4,
        Scroll = 5,
        Select = 6,
        StageClear = 7,
        Reset = 8,
        Hint = 9,
        Undo = 10,
        Star = 11,
        UnlockChapter = 12,
        // Flip Sounds
        Flip_Base = 100,
        Flip_Plus = 101,
        Flip_X = 102,
        Flip_Link = 103,
        Flip_Fixed = 104,
    }
    
    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount
    }

    public enum LKey
    {
        None,
        UI_GAMESTART,
        UI_QUIT,
        UI_OPTIONS,
        UI_CREDITS,
        UI_RESET,
        UI_UNLOCK,
        UI_WINDOW,
        UI_FULLSCREEN,
        UI_STAR,
        UI_NEXTSTAGE,
        UI_CHAPTER_UNLOCKED,
        TUTORIAL_BASE,
        TUTORIAL_COUNT,
        TUTORIAL_UNDO,
        TUTORIAL_HINT,
        TUTORIAL_SIZE,
        TUTORIAL_PLUS,
        TUTORIAL_X,
        TUTORIAL_FIXED,
        TUTORIAL_LINK,
        UI_RESET_CONFIRMATION,
        UI_CONFIRM,
        UI_CANCEL,
    }
}
