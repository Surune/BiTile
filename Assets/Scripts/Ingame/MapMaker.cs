using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapMaker : MonoBehaviour
{
    [SerializeField] private TMP_InputField mapInput;
    [SerializeField] private Button applyGridButton;
    [SerializeField] private Button hintModeButton;
    [SerializeField] private TMP_Text hintModeText;
    [SerializeField] private Button copyButton;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private Button tileButtonPrefab;

    private static readonly Color AccentColor = new Color(0.15f, 0.68f, 0.95f);
    private static readonly Color MutedColor = new Color(0.23f, 0.26f, 0.33f);
    private static readonly Color BlackTileColor = new Color(0.12f, 0.13f, 0.16f);
    private const int MaxSize = 12;

    private int rows = 3;
    private int columns = 3;
    private Vector2Int hintPosition;
    private TileInfo[,] tiles;
    private bool isHintMode;

    private void Awake()
    {
        CreateInitialTiles();
        applyGridButton.onClick.AddListener(ApplyGridSize);
        hintModeButton.onClick.AddListener(ToggleHintMode);
        copyButton.onClick.AddListener(CopyCsv);
        RefreshGrid();
    }

    private void CreateInitialTiles()
    {
        tiles = new TileInfo[MaxSize, MaxSize];
        for (var row = 0; row < MaxSize; row++)
        {
            for (var column = 0; column < MaxSize; column++)
            {
                TileInfo tile;
                tile.Type = '.';
                tile.Color = 'W';
                tiles[row, column] = tile;
            }
        }
    }

    private void ApplyGridSize()
    {
        var lines = mapInput.text.Replace("\r", string.Empty).Split('\n');
        if (lines.Length == 1)
        {
            rows = Mathf.RoundToInt(Mathf.Sqrt(lines[0].Length));
            columns = rows;
        }
        else
        {
            rows = lines.Length;
            columns = lines[0].Length;
        }

        CreateInitialTiles();
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                tiles[row, column].Type = lines.Length == 1
                    ? lines[0][row * columns + column]
                    : lines[row][column];
            }
        }

        hintPosition = default;
        RefreshGrid();
    }

    private void PaintTile(int row, int column)
    {
        if (isHintMode)
        {
            hintPosition.x = row;
            hintPosition.y = column;
            ToggleHintMode();
            RefreshGrid();
            return;
        }

        FlipTiles(row, column, tiles[row, column].Type);
        RefreshGrid();
    }

    private void FlipTiles(int row, int column, char type)
    {
        if (type == '!')
        {
            return;
        }

        if (type == '=')
        {
            for (var targetRow = 0; targetRow < rows; targetRow++)
            {
                for (var targetColumn = 0; targetColumn < columns; targetColumn++)
                {
                    if (tiles[targetRow, targetColumn].Type == '=')
                    {
                        FlipColor(targetRow, targetColumn);
                    }
                }
            }
            return;
        }

        for (var rowOffset = -1; rowOffset <= 1; rowOffset++)
        {
            for (var columnOffset = -1; columnOffset <= 1; columnOffset++)
            {
                var shouldFlip = type == '.'
                    || type == '+' && (rowOffset == 0 || columnOffset == 0)
                    || type == '*' && (rowOffset == columnOffset || rowOffset == -columnOffset);
                var targetRow = row + rowOffset;
                var targetColumn = column + columnOffset;
                if (shouldFlip && targetRow >= 0 && targetRow < rows && targetColumn >= 0 && targetColumn < columns)
                {
                    FlipColor(targetRow, targetColumn);
                }
            }
        }
    }

    private void FlipColor(int row, int column)
    {
        tiles[row, column].Color = tiles[row, column].Color == 'B' ? 'W' : 'B';
    }

    private void ToggleHintMode()
    {
        isHintMode = !isHintMode;
        hintModeButton.image.color = isHintMode ? AccentColor : MutedColor;
        hintModeText.text = isHintMode ? "힌트 지정: ON" : "힌트 지정: OFF";
    }

    private void RefreshGrid()
    {
        foreach (Transform child in gridLayout.transform)
        {
            Destroy(child.gameObject);
        }

        gridLayout.constraintCount = columns;
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                var targetRow = row;
                var targetColumn = column;
                var tile = tiles[row, column];
                var tileButton = Instantiate(tileButtonPrefab, gridLayout.transform);
                var tileText = tileButton.GetComponentInChildren<TMP_Text>();
                tileButton.image.color = tile.Color == 'B' ? BlackTileColor : Color.white;
                tileText.color = tile.Color == 'B' ? Color.white : Color.black;
                tileText.text = hintPosition.x == row && hintPosition.y == column ? $"{tile.Type}\nH" : tile.Type.ToString();
                tileButton.onClick.AddListener(() => PaintTile(targetRow, targetColumn));
            }
        }
    }

    private void CopyCsv()
    {
        var type = new char[rows * columns];
        var color = new char[rows * columns];
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                var index = row * columns + column;
                type[index] = tiles[row, column].Type;
                color[index] = tiles[row, column].Color;
            }
        }
        var csv = $",,,{rows},{columns},{new string(type)},{new string(color)},\"({hintPosition.x}, {hintPosition.y})\",";
        GUIUtility.systemCopyBuffer = csv;
        statusText.text = $"복사 완료\n{csv}";
    }
}
