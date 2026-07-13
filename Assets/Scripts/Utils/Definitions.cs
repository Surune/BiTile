public static class Definitions
{
    public static string LobbySceneName => "LobbyScene";
    public static string OptionSceneName => "OptionsScene";
    public static string ChapterSelectSceneName => "ChapterSelectScene";
    public static string StageSelectSceneName => "StageSelectScene";
    public static string GameSceneName => "MainGameScene";

    public enum SoundType
    {
        Bgm = 1,
        Music = 2,
        Decline = 3,
        Flip_Base = 4,
        Flip_Plus = 5,
        Flip_X = 6,
        Flip_Link = 7,
        Reset = 8,
        Hint = 9,
        StageClear = 10,
        Undo = 11,
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
        TUTORIAL_BASE,
        TUTORIAL_COUNT,
        TUTORIAL_UNDO,
        TUTORIAL_HINT,
        TUTORIAL_SIZE,
        TUTORIAL_PLUS,
        TUTORIAL_X,
        TUTORIAL_FIXED,
    }
}
