using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    private AdmobManager admobManager;
    private readonly PuzzleStageRepository stageRepository = new PuzzleStageRepository();
    private PuzzleStageData currentStageData;

    public int currentStage = 1;
    private int maxClicks = 1;
    private int currentClicks = 0;

    public bool clickable = true;

    public GameObject tilePrefab;
    private GameObject board;
    private GameObject retryButtonObject;
    private GameObject nextButtonObject;
    private GameObject hintButtonObject;
    private Button retryButton;
    private Button nextButton;
    private Button hintButton;
    private Button resetButton;
    private TMPro.TextMeshProUGUI stageText;
    private TMPro.TextMeshProUGUI limitTurnText;
    private TMPro.TextMeshProUGUI moveTurnText;

    private int retryCount = 0;
    private int retryAdNum = 10;

    public float tileSpacing = 1.0f;

    public TileScriptableObject[] tileInfoObjects;

    private TileInfo[,] stageInfo;
    private int width;
    private int height;

    private PuzzleTile[] puzzleTiles;
    private int currentSkinIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        BindSceneObjects();
        StartGame(GameManager.StageSelection.LoadStageNum);
    }

    private void BindSceneObjects()
    {
        board = GameObject.Find("Tiles");
        retryButtonObject = GameObject.Find("UI_Retry");
        nextButtonObject = GameObject.Find("UI_Next");
        hintButtonObject = GameObject.Find("HintButton");

        retryButton = retryButtonObject.GetComponent<Button>();
        nextButton = nextButtonObject.GetComponent<Button>();
        hintButton = hintButtonObject.GetComponent<Button>();
        resetButton = GameObject.Find("ResetButton").GetComponent<Button>();
        stageText = GameObject.Find("StageNumText").GetComponent<TMPro.TextMeshProUGUI>();
        limitTurnText = GameObject.Find("LimitTurnText").GetComponent<TMPro.TextMeshProUGUI>();
        moveTurnText = GameObject.Find("MoveTurnText").GetComponent<TMPro.TextMeshProUGUI>();
    }

    private void StartGame(int loadStageNum = 0)
    {
        admobManager = FindObjectOfType<AdmobManager>();

        retryButtonObject.SetActive(false);
        retryButton.onClick.AddListener(Retry);
        nextButtonObject.SetActive(false);
        nextButton.onClick.AddListener(LoadNextStage);
        hintButton.onClick.AddListener(ShowAdForHint);

        currentStage = loadStageNum;
        currentSkinIndex = PlayerPrefs.GetInt("TILE_SKIN", 0);

        LoadStage(currentStage);

        GameManager.Sound.Play("music", Definitions.Sound.Bgm);
    }

    private void LoadStage(int stage)
    {
        ColorManager.Instance.SetColor(stage);
        currentStageData = stageRepository.Load(stage);
        currentStage = currentStageData.StageNumber;
        maxClicks = currentStageData.MaxClicks;
        stageInfo = currentStageData.Tiles;
        width = currentStageData.Width;
        height = currentStageData.Height;

        stageText.text = $"{stage}";
        limitTurnText.text = $"{maxClicks}";
        moveTurnText.text = $"{currentClicks}";

        CreatePuzzle();
    }

    private float GetDistanceFromCenter(int x, int length)
    {
        return x - (length - 1) / 2.0f;
    }

    private void CreatePuzzle()
    {
        foreach (Transform child in board.transform)
        {
            Destroy(child.gameObject);
        }

        puzzleTiles = new PuzzleTile[width * height];
        var boardRect = board.GetComponent<RectTransform>();

        for (var row = 0; row < width; row++)
        {
            for (var col = 0; col < height; col++)
            {
                var type = stageInfo[row, col].Type;
                var color = stageInfo[row, col].Color;

                var x = GetDistanceFromCenter(row, width) * tileSpacing;
                var y = -GetDistanceFromCenter(col, height) * tileSpacing;

                var tileObject = Instantiate(tilePrefab, boardRect);
                tileObject.transform.localPosition = new Vector3(x, y, 0);

                var tile = tileObject.GetComponent<PuzzleTile>();
                tile.row = row;
                tile.col = col;
                tile.type = type;
                tile.color = color;
                tile.imageObject = tile.GetComponent<Image>();
                tile.puzzleManager = Instance;
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
                    'B' => ColorManager.Instance.tileColor,
                    'W' => Color.white,
                    _ => tile.imageObject.color
                };
            }
        }

        hintButton.interactable = retryCount > retryAdNum;

        var scale = 0.8f * Screen.width / 900;
        transform.localScale = new Vector3(scale, scale, 1);
    }

    private IEnumerator DelayedColorChange(Image img, Color color, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
        img.color = color;
    }

    private IEnumerator DelayedSpriteChange(Image img, Sprite sprite, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
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

    public bool ChangeTileColor(int row, int col, float delay = 0f)
    {
        if (row < 0 || row >= width || col < 0 || col >= height)
        {
            return false;
        }

        stageInfo[row, col].Color = stageInfo[row, col].Color == 'W' ? 'B' : 'W';

        var tile = puzzleTiles[row * width + col];
        tile.color = stageInfo[row, col].Color;
        tile.type = stageInfo[row, col].Type;

        if (tile.type == '!')
        {
            tile.color = 'W';
            StartCoroutine(tile.StartShake(delay));
        }
        else
        {
            StartCoroutine(tile.StartRotate(delay));
            if (tile.type == '.')
            {
                if (tile.color == 'B')
                {
                    StartCoroutine(DelayedSpriteChange(tile.imageObject, tileInfoObjects[GetIndexByType(tile.type)].whiteSprite, delay));
                    StartCoroutine(DelayedColorChange(tile.imageObject, ColorManager.Instance.tileColor, delay));
                }
                else if (tile.color == 'W')
                {
                    StartCoroutine(DelayedSpriteChange(tile.imageObject, tileInfoObjects[GetIndexByType(tile.type)].GetWhiteSkinSprite(currentSkinIndex), delay));
                    StartCoroutine(DelayedColorChange(tile.imageObject, Color.white, delay));
                }
            }
            else
            {
                if (tile.color == 'B')
                {
                    StartCoroutine(DelayedColorChange(tile.imageObject, ColorManager.Instance.tileColor, delay));
                }
                else if (tile.color == 'W')
                {
                    StartCoroutine(DelayedColorChange(tile.imageObject, Color.white, delay));
                }
            }
        }

        return true;
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
        if (!clickable)
        {
            return;
        }

        hintButton.interactable = false;
        currentClicks++;
        GameManager.Sound.Play("flip2");
        moveTurnText.text = $"{currentClicks}";

        if (CheckStageClear())
        {
            clickable = false;
            Invoke("SetNextButtonActive", 0.45f);
        }
        else if (currentClicks >= maxClicks)
        {
            clickable = false;
            Invoke("SetRetryButtonActive", 0.45f);
        }
    }

    private void SetNextButtonActive()
    {
        GameManager.Sound.Play("stageclear");
        nextButtonObject.transform.rotation = Quaternion.Euler(0, 270, 0);
        nextButtonObject.transform.DORotate(new Vector3(0, 0, 0), 0.5f);
        resetButton.interactable = false;
        nextButtonObject.SetActive(true);
    }

    private void SetRetryButtonActive()
    {
        retryButtonObject.transform.rotation = Quaternion.Euler(0, 270, 0);
        retryButtonObject.transform.DORotate(new Vector3(0, 0, 0), 0.5f);
        retryButtonObject.SetActive(true);
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

        GameManager.Sound.Play("undo1");
        currentClicks = 0;
        LoadStage(currentStage);
        board.SetActive(true);
        retryButtonObject.SetActive(false);

        clickable = true;
    }

    private void LoadNextStage()
    {
        if (currentStage % 50 == 0)
        {
            PlayerPrefs.SetInt($"HAVE_SKIN_{currentStage / 50}", 1);
        }

        currentStage++;
        if (currentStage > PlayerPrefs.GetInt("STAGE", 1))
        {
            PlayerPrefs.SetInt("STAGE", currentStage);
        }

        currentClicks = 0;
        retryCount = 0;
        LoadStage(currentStage);
        board.SetActive(true);
        nextButtonObject.SetActive(false);
        resetButton.interactable = true;

        clickable = true;
    }

    private void ShowAdForHint()
    {
        GameManager.UI.ShowPopupUI<UI_ShowAdAskScreen>();
    }

    public void ShowHint()
    {
        var outline = puzzleTiles[currentStageData.HintRow * width + currentStageData.HintColumn].gameObject.AddComponent<Outline>();
        outline.effectColor = Color.red;
        outline.effectDistance = new Vector2(7f, 7f);
    }
}
