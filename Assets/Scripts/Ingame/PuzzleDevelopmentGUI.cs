using UnityEngine;

public class PuzzleDevelopmentGUI : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private PuzzleManager puzzleManager;

    private void OnGUI()
    {
        if (GUILayout.Button("Reload", GUILayout.Width(200f), GUILayout.Height(50f)))
        {
            puzzleManager.ReloadLevelInfo();
        }
    }
#endif
}
