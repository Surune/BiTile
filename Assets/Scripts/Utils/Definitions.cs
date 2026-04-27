public static class Definitions
{
    public static string LobbySceneName => "LobbyScene";
    public static string StageSelectSceneName => "StageSelectScene";
    public static string GameSceneName => "GameScene";

    public enum SoundType
    {
        Bgm = 1,
        Music = 2,
        Decline = 3,
        Flip1 = 4,
        Flip2 = 5,
        Flip3 = 6,
        Flip4 = 7,
        Reset = 8,
        Select = 9,
        StageClear = 10,
        Undo = 11,
    }
    
    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount
    }
}
