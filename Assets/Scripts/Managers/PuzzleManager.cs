using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    private AdmobManager admobManager;
    private readonly PuzzleStageRepository stageRepository = new PuzzleStageRepository();
    private PuzzleStageData currentStageData;

    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private UI_Main ui;
    [SerializeField] private RectTransform board;
    [SerializeField] private float tileSpacing = 125f;
    [SerializeField] private TileScriptableObject[] tileInfoObjects;
    
    private Button retryButton;
    private Button nextButton;
    private Button hintButton;
    private Button resetButton;

    private int retryCount = 0;
    private int retryAdNum = 10;

    private int width;
    private int height;

    private PuzzleTile[] puzzleTiles;
    private TileInfo[,] stageInfo;

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

        BindSceneObjects();
        StartGame(GameManager.Instance.StageSelection.LoadStageNum);
    }

    private void BindSceneObjects()
    {
        retryButton = GameObject.Find("UI_Retry").GetComponent<Button>();
        nextButton = GameObject.Find("UI_Next").GetComponent<Button>();
        hintButton = GameObject.Find("HintButton").GetComponent<Button>();
        resetButton = GameObject.Find("ResetButton").GetComponent<Button>();
    }

    private void StartGame(int stage)
    {
        admobManager = FindObjectOfType<AdmobManager>();

        currentStage = stage;
        currentSkinIndex = PlayerPrefs.GetInt("TILE_SKIN", 0);
        
        retryButton.gameObject.SetActive(false);
        retryButton.onClick.AddListener(Retry);
        
        nextButton.gameObject.SetActive(false);
        nextButton.onClick.AddListener(LoadNextStage);
        
        hintButton.onClick.AddListener(ShowAdForHint);

        resetButton.interactable = false;
        resetButton.onClick.AddListener(Retry);

        LoadStage();

        GameManager.Instance.Sound.Play("music", Definitions.Sound.Bgm);
    }

    private void LoadStage()
    {
        currentStageData = stageRepository.Load(currentStage);
        currentStage = currentStageData.StageNumber;
        maxClicks = currentStageData.MaxClicks;
        currentClicks = 0;
        stageInfo = currentStageData.Tiles;
        width = currentStageData.Width;
        height = currentStageData.Height;
        tileColor = GameManager.Instance.Color.GetTileColor(currentStage);
        
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

                var tileObject = Instantiate(tilePrefab, board);
                tileObject.transform.localPosition = new Vector3(x, y, 0);

                var tile = tileObject.GetComponent<PuzzleTile>();
                tile.Init(Instance, row, col, type, color);
                puzzleTiles[row * width + col] = tile;

                var tileInfoIndex = GetIndexByType(type);
                var tileInfo = tileInfoObjects[tileInfoIndex];

                if (tileInfoIndex == 0 && color == 'W')
                {
                    tile.imageObject.sprite = tileInfo.GetWhiteSkinSprite(currentSkinIndex);
                }
                else
                {
                    tile.imageObject.sprite = tileInfo.whiteSprite;
                }

                tile.imageObject.color = color switch
                {
                    'B' => tileColor,
                    'W' => Color.white,
                    _ => tile.imageObject.color
                };
            }
        }

        hintButton.interactable = retryCount > retryAdNum;

        var scale = 0.8f * Screen.width / 900;
        transform.localScale = new Vector3(scale, scale, 1);
    }

    private static int ToMilliseconds(float seconds)
    {
        return Mathf.RoundToInt(seconds * 1000f);
    }

    private async Task DelayedColorChange(Image img, Color color, float delay)
    {
        await Task.Delay(ToMilliseconds(delay + 0.1f));
        img.color = color;
    }

    private async Task DelayedSpriteChange(Image img, Sprite sprite, float delay)
    {
        await Task.Delay(ToMilliseconds(delay + 0.1f));
        img.sprite = sprite;
    }

    public void ChangeTileSkin(int skinIndex)
    {
        currentSkinIndex = skinIndex;
        foreach (var tile in puzzleTiles)
        {
            if (tile.type == '.' && tile.color == 'W')
            {
                tile.imageObject.sprite = tileInfoObjects[0].GetWhiteSkinSprite(skinIndex);
            }
        }
    }

    public bool CanChangeTileColor(int row, int col)
    {
        return row >= 0 && row < width && col >= 0 && col < height;
    }

    public async Task ChangeTileColor(int row, int col, float delay = 0f)
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
                        DelayedSpriteChange(tile.imageObject, tileInfoObjects[GetIndexByType(tile.type)].whiteSprite, delay),
                        DelayedColorChange(tile.imageObject, tileColor, delay)
                    );
                }
                else if (tile.color == 'W')
                {
                    await Task.WhenAll(
                        rotateTask,
                        DelayedSpriteChange(tile.imageObject, tileInfoObjects[GetIndexByType(tile.type)].GetWhiteSkinSprite(currentSkinIndex), delay),
                        DelayedColorChange(tile.imageObject, Color.white, delay)
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
                        DelayedColorChange(tile.imageObject, tileColor, delay)
                    );
                }
                else if (tile.color == 'W')
                {
                    await Task.WhenAll(
                        rotateTask,
                        DelayedColorChange(tile.imageObject, Color.white, delay)
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

    public void TileClicked()
    {
        if (!isClickable)
        {
            return;
        }

        resetButton.interactable = true;
        hintButton.interactable = false;
        currentClicks++;
        GameManager.Instance.Sound.Play("flip2");
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
        GameManager.Instance.Sound.Play("stageclear");
        
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
        
        retryCount++;
        if (retryCount % retryAdNum == 0)
        {
            admobManager.ShowInterstitialAd();
        }

        GameManager.Instance.Sound.Play("undo1");
        currentClicks = 0;
        LoadStage();
        retryButton.gameObject.SetActive(false);
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
        retryCount = 0;
        LoadStage();
        nextButton.gameObject.SetActive(false);
        resetButton.interactable = true;

        isClickable = true;
    }

    private void ShowAdForHint()
    {
        admobManager.ShowRewardedAd(ShowHint);
    }

    private void ShowHint()
    {
        var outline = puzzleTiles[currentStageData.HintRow * width + currentStageData.HintColumn].gameObject.GetOrAddComponent<Outline>();
        outline.effectColor = Color.red;
        outline.effectDistance = new Vector2(7f, 7f);
    }
}
