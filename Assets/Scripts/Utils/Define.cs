using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum Scene
    {
        Unknown,
        LobbyScene,
        GameScene,
        WorldSelectScene,
        TutorialScene
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount
    }
    public enum Cameramode
    {
        QuarterView,
    }

    public enum UIEvent
    {
        Click,
        Drag,
    }
    public enum MouseEvent
    {
        Press,
        Click,
    }
}
