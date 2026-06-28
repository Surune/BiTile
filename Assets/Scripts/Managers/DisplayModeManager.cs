using System;
using UnityEngine;

public static class DisplayModeManager
{
    private const string FullScreenKey = "FullScreen";

    private static bool isFullScreen;

    public static event Action Changed = delegate { };

    public static bool IsFullScreen => isFullScreen;

    public static void Init()
    {
        isFullScreen = PlayerPrefs.GetInt(FullScreenKey, Convert.ToInt32(Screen.fullScreen)) == 1;
        Apply();
    }

    public static void ToggleFullScreen()
    {
        SetFullScreen(!IsFullScreen);
    }

    public static void SetFullScreen(bool fullScreen)
    {
        isFullScreen = fullScreen;
        Apply();
        PlayerPrefs.SetInt(FullScreenKey, Convert.ToInt32(fullScreen));
        PlayerPrefs.Save();
        Changed();
    }

    private static void Apply()
    {
        Screen.fullScreenMode = isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }
}
