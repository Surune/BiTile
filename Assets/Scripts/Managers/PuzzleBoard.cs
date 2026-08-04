using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PuzzleBoard : MonoBehaviour
{
    private const int ClickParticleCount = 6;

    private PuzzleStageRepository stageRepository;
    private PuzzleStageData currentStageData;

    [SerializeField] private PuzzleTile tilePrefab;
    [SerializeField] private UI_MainGame ui;
    [SerializeField] private Camera camera;
    [SerializeField] private Transform board;
    [SerializeField] private float tileSpacing = 125f;
    [SerializeField] private float stageTransitionHalfRotateDuration = 0.25f;
    [SerializeField] private TileScriptableObject[] tileInfoObjects;
    
    [Header("UI")]
    [SerializeField] private UI_StarNotification starNotification;
    [SerializeField] private Transform clearNotification;
    [SerializeField] private Button hintButton;
    [SerializeField] private ButtonKey undoButtonKey;
    [SerializeField] private ButtonKey resetButtonKey;
    [SerializeField] private Color enabledColor;
    [SerializeField] private Color disabledColor;

    [Header("Inputs")]
    [SerializeField] private InputActionReference undo;
    [SerializeField] private InputActionReference reset;
    [SerializeField] private InputActionReference confirm;
    [SerializeField] private InputActionReference click;

    [Header("Particles")]
    [SerializeField] private ParticleSystem successParticle;
    [SerializeField] private ParticleSystem clickParticle;

    private InputAction confirmInputAction;
    private InputAction clickInputAction;
    
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
    private Definitions.GameMode currentMode;
    private int currentChapter = 1;
    private int currentStage = 1;
    private int maxClicks = 1;
    private int currentClicks = 0;
    private bool acquiredStar;
    private bool unlockedNextStage;

    private void Awake()
    {
        confirmInputAction = confirm.action.Clone();
        clickInputAction = click.action.Clone();
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

        clickInputAction.performed += OnClickAction;
        clickInputAction.Enable();
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

        clickInputAction.performed -= OnClickAction;
        clickInputAction.Disable();
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
        TryLoadNextStage();
    }

    private void OnClickAction(InputAction.CallbackContext context)
    {
        var pointer = (Pointer)context.control.device;
        var ray = camera.ScreenPointToRay(pointer.position.ReadValue());
        var distance = -ray.origin.y / ray.direction.y;
        clickParticle.transform.position = ray.GetPoint(distance);
        clickParticle.Emit(ClickParticleCount);

        TryLoadNextStage();
    }

    private void TryLoadNextStage()
    {
        if (clearNotification.gameObject.activeInHierarchy)
        {
            LoadNextStage();
        }
    }

    private void StartGame(StageSelectionState stageSelection)
    {
        currentMode = stageSelection.Mode;
        stageRepository = new PuzzleStageRepository(currentMode);
        currentChapter = stageSelection.Chapter;
        currentStage = stageSelection.Stage;

        starNotification.Hide();
        clearNotification.gameObject.SetActive(false);
        
        hintButton.onClick.AddListener(ShowHint);

        resetButtonKey.Button.onClick.AddListener(Retry);
        OnOffResetButton(false);

        undoButtonKey.Button.onClick.AddListener(Undo);
        OnOffUndoButton(false);

        LoadStage();
    }

    private void LoadStage()
    {
        CancelInvoke(nameof(SetNextButtonActive));
        CancelInvoke(nameof(PlaySuccessParticle));
        CancelInvoke(nameof(SetStarNotificationActive));
        StopSuccessParticle();
        starNotification.Hide();
        clearNotification.gameObject.SetActive(false);
        OnOffResetButton(false);

        currentStageData = stageRepository.Load(currentChapter, currentStage);
        currentChapter = currentStageData.Chapter;
        currentStage = currentStageData.Stage;
        maxClicks = currentStageData.MaxClicks;
        currentClicks = 0;
        acquiredStar = false;
        unlockedNextStage = false;
        undoHistory.Clear();
        stageInfo = currentStageData.Tiles;
        width = currentStageData.Width;
        height = currentStageData.Height;
        var chapterData = GameManager.Instance.GetChapterData(currentChapter);
        GameManager.Instance.Sound.PlayBGM(chapterData.Bgm);
        tileColor = chapterData.TileColor;
        camera.backgroundColor = chapterData.BackgroundColor;
        
        ui.Init(currentChapter, currentStage, maxClicks, currentClicks, currentStageData.TutorialLkey, chapterData.BackgroundSprites);

        CreatePuzzle();
    }

#if UNITY_EDITOR
    public void ReloadLevelInfo()
    {
        stageRepository.Reload();
        LoadStage();
        isClickable = true;
        isTileClickInProgress = false;
        isStageTransitionInProgress = false;
    }
#endif

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

        if (currentStageData.ShowHint)
        {
            ShowHint(false);
        }
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

        if (stageInfo[row, col].Type == '!')
        {
            return;
        }

        stageInfo[row, col].Color = stageInfo[row, col].Color == 'W' ? 'B' : 'W';

        var tile = puzzleTiles[row * width + col];
        tile.color = stageInfo[row, col].Color;
        tile.type = stageInfo[row, col].Type;

        await Task.WhenAll(
            tile.StartRotate(delay),
            tile.RefreshColorWithDelay(delay)
        );
    }

    public Task ChangeLinkTiles(char linkType, float delayInterval)
    {
        var tasks = new List<Task>();
        var delay = 0f;

        for (var index = 0; index < puzzleTiles.Length; index++)
        {
            var tile = puzzleTiles[index];
            if (tile.type != linkType)
            {
                continue;
            }

            tasks.Add(ChangeTileColor(tile.row, tile.col, delay));
            delay += delayInterval;
        }

        return Task.WhenAll(tasks);
    }

    public Task ChangeAllTiles(float delayInterval)
    {
        var tasks = new List<Task>();
        var delay = 0f;

        for (var index = 0; index < puzzleTiles.Length; index++)
        {
            var tile = puzzleTiles[index];
            tasks.Add(ChangeTileColor(tile.row, tile.col, delay));
            delay += delayInterval;
        }

        return Task.WhenAll(tasks);
    }

    public static bool IsLinkType(char type)
    {
        return type == '(' || type == ')';
    }

    private int GetIndexByType(char type)
    {
        return type switch
        {
            '.' => 0,
            '+' => 1,
            '*' => 2,
            '!' => 3,
            '(' => 4,
            ')' => 5,
            '@' => 6,
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

    public void TileClicked(char type)
    {
        if (!isClickable)
        {
            return;
        }

        OnOffResetButton(true);
        HideHint();
        hintButton.interactable = false;
        currentClicks++;
        GameManager.Instance.Sound.PlaySFX(GetTileSoundType(type));
        ui.UpdateClicks(currentClicks, maxClicks);

        if (CheckStageClear())
        {
            isClickable = false;
            OnOffUndoButton(false);
            OnOffResetButton(false);
            acquiredStar = TryUnlockStageStar();
            unlockedNextStage = TryUnlockNextStage();
            TryUnlockChapterAchievements();
            Invoke(nameof(PlaySuccessParticle), 0.3f);
            Invoke(nameof(SetNextButtonActive), 0.5f);
            if (acquiredStar)
            {
                Invoke(nameof(SetStarNotificationActive), 1.5f);
            }
        }
    }

    private static Definitions.SoundType GetTileSoundType(char type)
    {
        if (IsLinkType(type))
        {
            return Definitions.SoundType.Flip_Link;
        }

        return type switch
        {
            '.' => Definitions.SoundType.Flip_Base,
            '+' => Definitions.SoundType.Flip_Plus,
            '*' => Definitions.SoundType.Flip_X,
            '@' => Definitions.SoundType.Flip_Base,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private bool TryUnlockStageStar()
    {
        if (currentClicks > maxClicks)
        {
            return false;
        }

        return SaveManager.UnlockStar(currentMode, currentChapter, currentStage);
    }

    private bool TryUnlockNextStage()
    {
        var clearedStageCount = SaveManager.GetClearedStageCount(currentMode, currentChapter);
        var clearedNewStage = currentStage > clearedStageCount;
        if (clearedNewStage)
        {
            SaveManager.SetClearedStageCount(currentMode, currentChapter, currentStage);
            if (currentMode == Definitions.GameMode.Normal &&
                currentStage == stageRepository.GetStageCount(currentChapter))
            {
                SaveManager.UnlockHardMode();
            }
        }

        var nextProgressStage = stageRepository.GetProgressStage(currentChapter, currentStage) + 1;
        if (nextProgressStage > stageRepository.TotalStageCount)
        {
            return false;
        }

        var nextChapter = stageRepository.GetChapter(nextProgressStage);
        if (currentMode == Definitions.GameMode.Hard && nextChapter != currentChapter)
        {
            var normalStageRepository = new PuzzleStageRepository(Definitions.GameMode.Normal);
            return SaveManager.IsChapterCleared(
                Definitions.GameMode.Normal,
                nextChapter,
                normalStageRepository.GetStageCount(nextChapter));
        }

        return clearedNewStage;
    }

    private void TryUnlockChapterAchievements()
    {
        if (currentMode == Definitions.GameMode.Hard)
        {
            return;
        }

        var currentProgressStage = stageRepository.GetProgressStage(currentChapter, currentStage);
        var isLastStageInChapter = currentProgressStage == stageRepository.TotalStageCount
                                   || stageRepository.GetChapter(currentProgressStage + 1) != currentChapter;
        if (isLastStageInChapter)
        {
            SteamManager.UnlockAchievement($"ACHIEVEMENT_CHAPTER_{currentChapter}_CLEAR");
        }

        var stageCount = stageRepository.GetStageCount(currentChapter);
        for (var stage = 1; stage <= stageCount; stage++)
        {
            if (!SaveManager.HasStar(currentMode, currentChapter, stage))
            {
                return;
            }
        }

        SteamManager.UnlockAchievement($"ACHIEVEMENT_CHAPTER_{currentChapter}_PERFECT");
    }

    private void SetNextButtonActive()
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.StageClear);
        
        clearNotification.transform.rotation = Quaternion.Euler(0, 270, 0);
        clearNotification.transform.DORotate(new Vector3(0, 0, 0), 0.5f);
        clearNotification.gameObject.SetActive(true);

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
        starNotification.Play();
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

        starNotification.Hide();
        clearNotification.gameObject.SetActive(false);
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

        var progressStage = stageRepository.GetProgressStage(currentChapter, currentStage) + 1;
        if (progressStage > stageRepository.TotalStageCount)
        {
            starNotification.Hide();
            clearNotification.gameObject.SetActive(false);
            GameManager.Instance.SetChapter(currentChapter);
            SceneManager.LoadScene(Definitions.ChapterSelectSceneName);
            return;
        }

        var nextChapter = stageRepository.GetChapter(progressStage);

        isStageTransitionInProgress = true;
        starNotification.Hide();
        clearNotification.gameObject.SetActive(false);
        hintButton.interactable = false;
        OnOffResetButton(false);
        OnOffUndoButton(false);

        isClickable = false;

        if (currentMode == Definitions.GameMode.Hard && nextChapter != currentChapter)
        {
            GameManager.Instance.SetChapter(unlockedNextStage ? nextChapter : currentChapter);
            if (unlockedNextStage)
            {
                UI_ChapterSelect.OpenStageSelectOnAwake = true;
            }

            SceneManager.LoadScene(Definitions.ChapterSelectSceneName);
            return;
        }

        if (nextChapter != currentChapter && unlockedNextStage)
        {
            CancelInvoke(nameof(SetStarNotificationActive));
            await ui.PlayChapterUnlock(nextChapter);
            GameManager.Instance.SetChapter(nextChapter);
            UI_ChapterSelect.OpenStageSelectOnAwake = true;
            SceneManager.LoadScene(Definitions.ChapterSelectSceneName);
            return;
        }

        await board.DOLocalRotate(Vector3.forward * 90f, stageTransitionHalfRotateDuration).SetEase(Ease.InQuad).AsyncWaitForCompletion();

        currentChapter = nextChapter;
        currentStage = stageRepository.GetStage(progressStage);
        GameManager.Instance.SetStage(currentChapter, currentStage);

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
        ShowHint(true);
    }

    private void ShowHint(bool playSfx)
    {
        hintTile = puzzleTiles[currentStageData.HintPosition.x * width + currentStageData.HintPosition.y];
        hintTile.ShowHint();
        if (playSfx)
        {
            GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Hint);
        }

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
        clickInputAction.Dispose();
    }
}

[Serializable]
public struct ButtonKey
{
    public Button Button;
    public Image KeyImage;
}
