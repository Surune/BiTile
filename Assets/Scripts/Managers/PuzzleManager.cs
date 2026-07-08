using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    private readonly PuzzleStageRepository stageRepository = new PuzzleStageRepository();
    private PuzzleStageData currentStageData;

    [SerializeField] private PuzzleTile tilePrefab;
    [SerializeField] private UI_MainGame ui;
    [SerializeField] private Camera camera;
    [SerializeField] private Transform board;
    [SerializeField] private ParticleSystem successParticle;
    [SerializeField] private float tileSpacing = 125f;
    [SerializeField] private float stageTransitionHalfRotateDuration = 0.25f;
    [SerializeField] private TileScriptableObject[] tileInfoObjects;
    
    [Header("UI")]
    [SerializeField] private Transform starNotification;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button hintButton;
    [SerializeField] private ButtonKey undoButtonKey;
    [SerializeField] private ButtonKey resetButtonKey;
    [SerializeField] private Color enabledColor;
    [SerializeField] private Color disabledColor;

    [Header("Inputs")]
    [SerializeField] private InputActionReference undo;
    [SerializeField] private InputActionReference reset;
    [SerializeField] private InputActionReference confirmAction;

    private InputAction confirmInputAction;
    
    private int width;
    private int height;

    private PuzzleTile[] puzzleTiles;
    private PuzzleTile hintTile;
    private bool isHintShown;
    private TileInfo[,] stageInfo;
    private readonly Stack<char[]> undoHistory = new Stack<char[]>();

    private bool CanAcceptTileClick => isClickable && !isTileClickInProgress;
    private bool isClickable = true;
    private bool isTileClickInProgress;
    private bool isStageTransitionInProgress;

    private Color tileColor;
    private int currentChapter = 1;
    private int currentStage = 1;
    private int maxClicks = 1;
    private int currentClicks = 0;
    private bool acquiredStar;

    private void Awake()
    {
        confirmInputAction = confirmAction.action.Clone();
        StartGame(GameManager.Instance.StageSelection);
    }

    private void OnEnable()
    {
        undo.action.performed += OnUndoAction;
        undo.action.Enable();

        reset.action.performed += OnResetAction;
        reset.action.Enable();

        confirmInputAction.performed += OnConfirmAction;
        confirmInputAction.Enable();
    }

    private void OnDisable()
    {
        StopSuccessParticle();

        undo.action.performed -= OnUndoAction;
        undo.action.Disable();

        reset.action.performed -= OnResetAction;
        reset.action.Disable();

        confirmInputAction.performed -= OnConfirmAction;
        confirmInputAction.Disable();
    }

    private void OnUndoAction(InputAction.CallbackContext context)
    {
        if (undoButtonKey.Button.interactable)
        {
            Undo();
        }
    }

    private void OnResetAction(InputAction.CallbackContext context)
    {
        if (resetButtonKey.Button.interactable)
        {
            Retry();
        }
    }

    private void OnConfirmAction(InputAction.CallbackContext context)
    {
        if (nextButton.gameObject.activeInHierarchy)
        {
            LoadNextStage();
        }
    }

    private void StartGame(StageSelectionState stageSelection)
    {
        currentChapter = stageSelection.Chapter;
        currentStage = stageSelection.Stage;
        
        starNotification.gameObject.SetActive(false);
        
        nextButton.gameObject.SetActive(false);
        nextButton.onClick.AddListener(LoadNextStage);
        
        hintButton.onClick.AddListener(ShowHint);

        resetButtonKey.Button.onClick.AddListener(Retry);
        OnOffResetButton(false);

        undoButtonKey.Button.onClick.AddListener(Undo);
        OnOffUndoButton(false);

        LoadStage();

        GameManager.Instance.Sound.PlayBGM(Definitions.SoundType.Music);
    }

    private void LoadStage()
    {
        CancelInvoke(nameof(SetNextButtonActive));
        CancelInvoke(nameof(PlaySuccessParticle));
        CancelInvoke(nameof(SetStarNotificationActive));
        StopSuccessParticle();
        starNotification.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        OnOffResetButton(false);

        currentStageData = stageRepository.Load(currentChapter, currentStage);
        currentChapter = currentStageData.Chapter;
        currentStage = currentStageData.Stage;
        maxClicks = currentStageData.MaxClicks;
        currentClicks = 0;
        acquiredStar = false;
        undoHistory.Clear();
        stageInfo = currentStageData.Tiles;
        width = currentStageData.Width;
        height = currentStageData.Height;
        var chapterData = GameManager.Instance.Chapter.GetData(currentChapter);
        tileColor = chapterData.TileColor;
        camera.backgroundColor = chapterData.BackgroundColor;
        
        ui.Init(currentChapter, currentStage, maxClicks, currentClicks, currentStageData.TutorialLkey, chapterData.BackgroundSprites);

        CreatePuzzle();
    }

    private float GetDistanceFromCenter(int x, int length)
    {
        return x - (length - 1) / 2.0f;
    }

    private void CreatePuzzle()
    {
        HideHint();

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

                var x = GetDistanceFromCenter(col, width) * tileSpacing;
                var y = -GetDistanceFromCenter(row, height) * tileSpacing;
                var pos = new Vector3(x, 0, y);

                var tile = Instantiate(tilePrefab, board);
                tile.transform.SetLocalPositionAndRotation(pos, Quaternion.identity);
                tile.Init(this, row, col, type, color, tileInfoObjects[GetIndexByType(type)], tileColor);
                puzzleTiles[row * width + col] = tile;
            }
        }

        hintButton.interactable = true;
        OnOffUndoButton(false);
    }

    public bool CanChangeTileColor(int row, int col)
    {
        return row >= 0 && row < width && col >= 0 && col < height;
    }

    public bool TryBeginTileClick()
    {
        if (!CanAcceptTileClick)
        {
            return false;
        }

        isTileClickInProgress = true;
        return true;
    }

    public void CompleteTileClick()
    {
        isTileClickInProgress = false;
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
            return;
        }

        await Task.WhenAll(
            tile.StartRotate(delay),
            tile.RefreshColorWithDelay(delay)
        );
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
        OnOffUndoButton(true);
    }

    private void OnOffUndoButton(bool isOn)
    {
        undoButtonKey.Button.interactable = isOn;
        undoButtonKey.KeyImage.color = isOn ? enabledColor : disabledColor;
    }

    private void OnOffResetButton(bool isOn)
    {
        resetButtonKey.Button.interactable = isOn;
        resetButtonKey.KeyImage.color = isOn ? enabledColor : disabledColor;
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

        OnOffResetButton(true);
        HideHint();
        hintButton.interactable = false;
        currentClicks++;
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Flip2);
        ui.UpdateClicks(currentClicks, maxClicks);

        if (CheckStageClear())
        {
            isClickable = false;
            OnOffUndoButton(false);
            OnOffResetButton(false);
            acquiredStar = TryUnlockStageStar();
            Invoke(nameof(PlaySuccessParticle), 0.4f);
            Invoke(nameof(SetNextButtonActive), 0.45f);
        }
    }

    private bool TryUnlockStageStar()
    {
        if (currentClicks > maxClicks)
        {
            return false;
        }

        var progressStage = PuzzleStageRepository.GetProgressStage(currentChapter, currentStage);
        return SaveManager.UnlockStar(progressStage);
    }

    private void SetNextButtonActive()
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.StageClear);
        
        nextButton.transform.rotation = Quaternion.Euler(0, 270, 0);
        nextButton.transform.DORotate(new Vector3(0, 0, 0), 0.5f);
        nextButton.gameObject.SetActive(true);

        if (acquiredStar)
        {
            SetStarNotificationActive();
        }
        
        OnOffUndoButton(false);
        OnOffResetButton(false);
    }

    private void PlaySuccessParticle()
    {
        StopSuccessParticle();
        successParticle.Play();
    }

    private void StopSuccessParticle()
    {
        successParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void SetStarNotificationActive()
    {
        starNotification.rotation = Quaternion.Euler(0, 270, 0);
        starNotification.DORotate(new Vector3(0, 0, 0), 0.5f);
        starNotification.gameObject.SetActive(true);
    }

    public void Retry()
    {
        if (isTileClickInProgress || currentClicks <= 0)
        {
            return;
        }
        
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Reset);
        currentClicks = 0;
        undoHistory.Clear();
        LoadStage();
        isClickable = true;
    }

    private async void Undo()
    {
        if (isTileClickInProgress || currentClicks <= 0 || undoHistory.Count <= 0)
        {
            return;
        }

        CancelInvoke(nameof(SetNextButtonActive));
        CancelInvoke(nameof(PlaySuccessParticle));
        CancelInvoke(nameof(SetStarNotificationActive));

        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Undo);
        isClickable = false;
        OnOffUndoButton(false);
        currentClicks--;
        acquiredStar = false;
        ui.UpdateClicks(currentClicks, maxClicks);
        await RestoreTileColors(undoHistory.Pop());

        starNotification.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        hintButton.interactable = currentClicks == 0;
        OnOffResetButton(currentClicks > 0);
        OnOffUndoButton(undoHistory.Count > 0);
        isClickable = true;
    }

    private async void LoadNextStage()
    {
        if (isStageTransitionInProgress)
        {
            return;
        }

        StopSuccessParticle();

        var progressStage = PuzzleStageRepository.GetProgressStage(currentChapter, currentStage) + 1;
        if (progressStage > PuzzleStageRepository.TotalStageCount)
        {
            starNotification.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);
            SceneManager.LoadScene(Definitions.ChapterSelectSceneName);
            return;
        }

        isStageTransitionInProgress = true;
        starNotification.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        hintButton.interactable = false;
        OnOffResetButton(false);
        OnOffUndoButton(false);

        isClickable = false;
        await board.DOLocalRotate(Vector3.forward * 90f, stageTransitionHalfRotateDuration).SetEase(Ease.InQuad).AsyncWaitForCompletion();

        currentChapter = PuzzleStageRepository.GetChapter(progressStage);
        currentStage = PuzzleStageRepository.GetStage(progressStage);
        if (progressStage > SaveManager.LastUnlockedStage)
        {
            SaveManager.LastUnlockedStage = progressStage;
        }

        currentClicks = 0;
        undoHistory.Clear();
        LoadStage();

        board.localRotation = Quaternion.Euler(0f, 0f, -90f);
        await board.DOLocalRotate(Vector3.zero, stageTransitionHalfRotateDuration).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
        board.localRotation = Quaternion.identity;

        isStageTransitionInProgress = false;
        isClickable = true;
    }

    private void ShowHint()
    {
        hintTile = puzzleTiles[currentStageData.HintPosition.x * width + currentStageData.HintPosition.y];
        hintTile.ShowHint();
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Hint);
        isHintShown = true;
        hintButton.interactable = false;
    }

    private void HideHint()
    {
        if (!isHintShown)
        {
            return;
        }

        hintTile.HideHint();
        isHintShown = false;
    }

    private void OnDestroy()
    {
        confirmInputAction.Dispose();
    }
}

[Serializable]
public struct ButtonKey
{
    public Button Button;
    public Image KeyImage;
}
