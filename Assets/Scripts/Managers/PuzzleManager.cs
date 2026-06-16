using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    private readonly PuzzleStageRepository stageRepository = new PuzzleStageRepository();
    private PuzzleStageData currentStageData;

    [SerializeField] private PuzzleTile tilePrefab;
    [SerializeField] private UI_Main ui;
    [SerializeField] private Camera camera;
    [SerializeField] private Transform board;
    [SerializeField] private float tileSpacing = 125f;
    [SerializeField] private TileScriptableObject[] tileInfoObjects;
    
    [Header("Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button hintButton;
    [SerializeField] private Button resetButton;
    
    private int width;
    private int height;

    private PuzzleTile[] puzzleTiles;
    private TileInfo[,] stageInfo;
    private readonly Stack<char[]> undoHistory = new Stack<char[]>();

    public bool IsClickable => isClickable;
    private bool isClickable = true;
    
    private Color tileColor;
    private int currentStage = 1;
    private int maxClicks = 1;
    private int currentClicks = 0;
    private int currentSkinIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        StartGame(GameManager.Instance.StageSelection.LoadStageNum);
    }

    private void StartGame(int stage)
    {
        currentStage = stage;
        currentSkinIndex = PlayerPrefs.GetInt("TILE_SKIN", 0);
        
        retryButton.gameObject.SetActive(false);
        retryButton.onClick.AddListener(Retry);
        
        nextButton.gameObject.SetActive(false);
        nextButton.onClick.AddListener(LoadNextStage);
        
        hintButton.onClick.AddListener(ShowHint);

        resetButton.interactable = false;
        resetButton.onClick.AddListener(Retry);

        ui.UndoButton.interactable = false;
        ui.UndoButton.onClick.AddListener(Undo);

        LoadStage();

        GameManager.Instance.Sound.PlayBGM(Definitions.SoundType.Music);
    }

    private void LoadStage()
    {
        currentStageData = stageRepository.Load(currentStage);
        currentStage = currentStageData.StageNumber;
        maxClicks = currentStageData.MaxClicks;
        currentClicks = 0;
        undoHistory.Clear();
        stageInfo = currentStageData.Tiles;
        width = currentStageData.Width;
        height = currentStageData.Height;
        tileColor = GameManager.Instance.Color.GetTileColor(currentStage);
        camera.backgroundColor = GameManager.Instance.Color.GetBackgroundColor(currentStage);
        
        ui.Init(currentStage, maxClicks, currentClicks);

        CreatePuzzle();
    }

    private float GetDistanceFromCenter(int x, int length)
    {
        return x - (length - 1) / 2.0f;
    }

    private void CreatePuzzle()
    {
        foreach (Transform child in board)
        {
            Destroy(child.gameObject);
        }

        puzzleTiles = new PuzzleTile[width * height];

        for (var row = 0; row < width; row++)
        {
            for (var col = 0; col < height; col++)
            {
                var type = stageInfo[row, col].Type;
                var color = stageInfo[row, col].Color;

                var x = GetDistanceFromCenter(row, width) * tileSpacing;
                var y = -GetDistanceFromCenter(col, height) * tileSpacing;
                var pos = new Vector3(x, 0, y);

                var tile = Instantiate(tilePrefab, pos, Quaternion.identity, board);
                tile.Init(Instance, row, col, type, color, tileInfoObjects[GetIndexByType(type)].model, tileColor);
                puzzleTiles[row * width + col] = tile;
            }
        }

        hintButton.interactable = true;
        ui.UndoButton.interactable = false;
    }

    public void ChangeTileSkin(int skinIndex)
    {
        throw new NotImplementedException();
        /*
        currentSkinIndex = skinIndex;
        foreach (var tile in puzzleTiles)
        {
            if (tile.type == '.' && tile.color == 'W')
            {
                tile.imageObject.sprite = tileInfoObjects[0].GetWhiteSkinSprite(skinIndex);
            }
        }
        */
    }

    public bool CanChangeTileColor(int row, int col)
    {
        return row >= 0 && row < width && col >= 0 && col < height;
    }

    public async Task ChangeTileColor(int row, int col, float delay)
    {
        if (!CanChangeTileColor(row, col))
        {
            return;
        }

        stageInfo[row, col].Color = stageInfo[row, col].Color == 'W' ? 'B' : 'W';

        var tile = puzzleTiles[row * width + col];
        tile.color = stageInfo[row, col].Color;
        tile.type = stageInfo[row, col].Type;

        if (tile.type == '!')
        {
            tile.color = 'W';
            await tile.StartShake(delay);
        }
        else
        {
            var rotateTask = tile.StartRotate(delay);
            if (tile.type == '.')
            {
                if (tile.color == 'B')
                {
                    await Task.WhenAll(
                        rotateTask,
                        //DelayedSpriteChange(tile.imageObject, tileInfoObjects[GetIndexByType(tile.type)].whiteSprite, delay),
                        tile.RefreshColorWithDelay(delay)
                    );
                }
                else if (tile.color == 'W')
                {
                    await Task.WhenAll(
                        rotateTask,
                        //DelayedSpriteChange(tile.imageObject, tileInfoObjects[GetIndexByType(tile.type)].GetWhiteSkinSprite(currentSkinIndex), delay),
                        tile.RefreshColorWithDelay(delay)
                    );
                }
                else
                {
                    await rotateTask;
                }
            }
            else
            {
                if (tile.color == 'B')
                {
                    await Task.WhenAll(
                        rotateTask,
                        tile.RefreshColorWithDelay(delay)
                    );
                }
                else if (tile.color == 'W')
                {
                    await Task.WhenAll(
                        rotateTask,
                        tile.RefreshColorWithDelay(delay)
                    );
                }
                else
                {
                    await rotateTask;
                }
            }
        }
    }

    private int GetIndexByType(char type)
    {
        return type switch
        {
            '.' => 0,
            '+' => 1,
            '*' => 2,
            '!' => 3,
            _ => -1
        };
    }

    private bool CheckStageClear()
    {
        return puzzleTiles.All(tile => tile.color == 'W');
    }

    public void RecordUndoState()
    {
        undoHistory.Push(CaptureTileColors());
        ui.UndoButton.interactable = true;
    }

    private char[] CaptureTileColors()
    {
        var colors = new char[width * height];
        for (var row = 0; row < width; row++)
        {
            for (var col = 0; col < height; col++)
            {
                colors[row * width + col] = puzzleTiles[row * width + col].color;
            }
        }

        return colors;
    }

    private async Task RestoreTileColors(char[] colors)
    {
        var tasks = new List<Task>();
        var delay = 0f;
        const float delayInterval = 0.02f;

        for (var row = 0; row < width; row++)
        {
            for (var col = 0; col < height; col++)
            {
                var color = colors[row * width + col];
                var tile = puzzleTiles[row * width + col];
                if (tile.color != color)
                {
                    var tileDelay = delay;
                    tasks.Add(tile.StartUndoRotate(tileDelay));
                    tasks.Add(tile.SetColorWithDelay(color, tileDelay));
                    delay += delayInterval;
                }

                stageInfo[row, col].Color = color;
            }
        }

        await Task.WhenAll(tasks);
    }

    public void TileClicked()
    {
        if (!isClickable)
        {
            return;
        }

        resetButton.interactable = true;
        hintButton.interactable = false;
        currentClicks++;
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Flip2);
        ui.UpdateClicks(maxClicks, currentClicks);

        if (CheckStageClear())
        {
            isClickable = false;
            Invoke("SetNextButtonActive", 0.45f);
        }
        else if (currentClicks >= maxClicks)
        {
            isClickable = false;
            Invoke("SetRetryButtonActive", 0.45f);
        }
    }

    private void SetNextButtonActive()
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.StageClear);
        
        nextButton.transform.rotation = Quaternion.Euler(0, 270, 0);
        nextButton.transform.DORotate(new Vector3(0, 0, 0), 0.5f);
        resetButton.interactable = false;
        nextButton.gameObject.SetActive(true);
    }

    private void SetRetryButtonActive()
    {
        retryButton.transform.rotation = Quaternion.Euler(0, 270, 0);
        retryButton.transform.DORotate(new Vector3(0, 0, 0), 0.5f);
        retryButton.gameObject.SetActive(true);
    }

    public void Retry()
    {
        if (currentClicks <= 0)
        {
            return;
        }
        
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Reset);
        currentClicks = 0;
        undoHistory.Clear();
        LoadStage();
        retryButton.gameObject.SetActive(false);
        isClickable = true;
    }

    public async void Undo()
    {
        if (currentClicks <= 0 || undoHistory.Count <= 0)
        {
            return;
        }

        CancelInvoke(nameof(SetNextButtonActive));
        CancelInvoke(nameof(SetRetryButtonActive));

        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Undo);
        isClickable = false;
        ui.UndoButton.interactable = false;
        currentClicks--;
        await RestoreTileColors(undoHistory.Pop());
        ui.UpdateClicks(maxClicks, currentClicks);

        retryButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        resetButton.interactable = currentClicks > 0;
        hintButton.interactable = currentClicks == 0;
        ui.UndoButton.interactable = undoHistory.Count > 0;
        isClickable = true;
    }

    private void LoadNextStage()
    {
        currentStage++;
        if (currentStage > PlayerPrefs.GetInt("STAGE", 1))
        {
            PlayerPrefs.SetInt("STAGE", currentStage);
        }

        currentClicks = 0;
        undoHistory.Clear();
        LoadStage();
        nextButton.gameObject.SetActive(false);
        resetButton.interactable = true;

        isClickable = true;
    }

    private void ShowHint()
    {
        throw new NotImplementedException();
        /*
        var outline = puzzleTiles[currentStageData.HintRow * width + currentStageData.HintColumn].gameObject.GetOrAddComponent<Outline>();
        outline.effectColor = Color.red;
        outline.effectDistance = new Vector2(7f, 7f);
        */
    }
}
