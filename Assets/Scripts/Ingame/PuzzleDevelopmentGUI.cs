using UnityEngine;

public class PuzzleDevelopmentGUI : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private PuzzleBoard puzzleBoard;

    private void OnGUI()
    {
        if (GUILayout.Button("Reload", GUILayout.Width(200f), GUILayout.Height(50f)))
        {
            puzzleBoard.ReloadLevelInfo();
        }
    }
#endif
}
