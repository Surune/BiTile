using System;
using UnityEngine;

public static class DisplayModeManager
{
    private const string FullScreenKey = "FullScreen";
    private const string ResolutionIndexKey = "ResolutionIndex";
    private const int DefaultResolutionIndex = 2;

    private static readonly int[] SupportedWidths =
    {
        1280,
        1600,
        1920,
        2560,
        3840
    };

    private static readonly int[] SupportedHeights =
    {
        720,
        900,
        1080,
        1440,
        2160
    };

    private static bool isFullScreen;
    private static int resolutionIndex;

    public static event Action Changed = delegate { };

#if UNITY_WEBGL && !UNITY_EDITOR
    public static bool IsFullScreen => Screen.fullScreen;
    public static string ResolutionLabel => $"{Screen.width} X {Screen.height}";
    public static bool CanSelectPreviousResolution => false;
    public static bool CanSelectNextResolution => false;
#else
    public static bool IsFullScreen => isFullScreen;
    public static string ResolutionLabel => $"{SupportedWidths[resolutionIndex]} X {SupportedHeights[resolutionIndex]}";
    public static bool CanSelectPreviousResolution => resolutionIndex > 0;
    public static bool CanSelectNextResolution => resolutionIndex < SupportedWidths.Length - 1;
#endif

    public static void Init()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // The Web template owns the canvas resolution and initial fullscreen state.
        isFullScreen = Screen.fullScreen;
#else
        isFullScreen = PlayerPrefs.GetInt(FullScreenKey, Convert.ToInt32(Screen.fullScreen)) == 1;
        resolutionIndex = PlayerPrefs.GetInt(ResolutionIndexKey, DefaultResolutionIndex);
        Apply();
#endif
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

    public static void SelectPreviousResolution()
    {
        if (!CanSelectPreviousResolution)
        {
            return;
        }

        resolutionIndex--;
        SaveResolution();
    }

    public static void SelectNextResolution()
    {
        if (!CanSelectNextResolution)
        {
            return;
        }

        resolutionIndex++;
        SaveResolution();
    }

    private static void Apply()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Screen.fullScreen = isFullScreen;
#else
        var fullScreenMode = isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(SupportedWidths[resolutionIndex], SupportedHeights[resolutionIndex], fullScreenMode);
#endif
    }

    private static void SaveResolution()
    {
        Apply();
        PlayerPrefs.SetInt(ResolutionIndexKey, resolutionIndex);
        PlayerPrefs.Save();
        Changed();
    }
}
