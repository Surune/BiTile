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
        Lobby = 1,
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

    public enum GameMode
    {
        Normal,
        Hard
    }
    
    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount
    }

    public enum LKey
    {
        None = -1,
        // UI ; do not change the order
        UI_GAMESTART = 1,
        UI_QUIT,
        UI_OPTIONS,
        UI_CREDITS,
        UI_RESET,
        UI_STAGECLEAR,
        UI_WINDOW,
        UI_FULLSCREEN,
        UI_STAR,
        UI_CONTINUE,
        UI_CHAPTER_UNLOCKED,
        UI_RESET_CONFIRMATION,
        UI_CONFIRM,
        UI_CANCEL,
        UI_NORMAL_MODE,
        UI_HARD_MODE,
        UI_WISHLIST,
        // TUTORIAL
        TUTORIAL_BASE = 1000,
        TUTORIAL_COUNT,
        TUTORIAL_UNDO,
        TUTORIAL_HINT,
        TUTORIAL_SIZE,
        TUTORIAL_PLUS,
        TUTORIAL_X,
        TUTORIAL_FIXED,
        TUTORIAL_LINK_ONE,
        TUTORIAL_LINK_TWO,
        TUTORIAL_LINK_EACH,
        TUTORIAL_ALL,
    }
}
