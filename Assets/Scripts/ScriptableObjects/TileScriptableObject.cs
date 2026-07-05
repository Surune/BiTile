using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TileScriptableObject", order = 1)]
public class TileScriptableObject : ScriptableObject
{
    public GameObject model;
    public bool[] flipPattern;

    public Task ChangeTiles(PuzzleManager puzzleManager, int row, int col, float delayInterval)
    {
        var tasks = new List<Task>();
        var delay = 0f;

        for (var index = 0; index < flipPattern.Length; index++)
        {
            if (!flipPattern[index])
            {
                continue;
            }

            var targetRow = row + index / 3 - 1;
            var targetCol = col + index % 3 - 1;
            if (!puzzleManager.CanChangeTileColor(targetRow, targetCol))
            {
                continue;
            }

            tasks.Add(puzzleManager.ChangeTileColor(targetRow, targetCol, delay));
            delay += delayInterval;
        }

        return Task.WhenAll(tasks);
    }
}
