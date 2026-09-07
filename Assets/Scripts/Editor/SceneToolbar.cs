using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;

[InitializeOnLoad]
public static class SceneToolbar
{
    private const string LobbyButtonPath = "BiTile/Lobby";
    private const string MapMakerButtonPath = "BiTile/MapMaker";

    static SceneToolbar()
    {
        EditorApplication.playModeStateChanged += _ =>
        {
            MainToolbar.Refresh(LobbyButtonPath);
            MainToolbar.Refresh(MapMakerButtonPath);
        };
    }

    [MainToolbarElement(LobbyButtonPath, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = 1)]
    private static MainToolbarElement CreateLobbyButton()
    {
        return CreateButton("Lobby", "Assets/Scenes/LobbyScene.unity");
    }

    [MainToolbarElement(MapMakerButtonPath, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = 2)]
    private static MainToolbarElement CreateMapMakerButton()
    {
        return CreateButton("MapMaker", "Assets/Scenes/MapMakerScene.unity");
    }

    private static MainToolbarButton CreateButton(string label, string scenePath)
    {
        MainToolbarContent content = default;
        content.text = label;
        content.tooltip = $"Open {label} scene";

        return new MainToolbarButton(content, () => OpenScene(scenePath))
        {
            enabled = !EditorApplication.isPlayingOrWillChangePlaymode
        };
    }

    private static void OpenScene(string scenePath)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
    }
}
