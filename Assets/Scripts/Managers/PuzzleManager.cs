using System.Collections;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;
    private AdmobManager admobManager;

    public int currentStage = 1;
    private int maxClicks = 1;
    private int currentClicks = 0;

    public bool clickable = true;

    private GameObject board;
    
    public GameObject tilePrefab;
    private GameObject retryButton;
    private GameObject nextButton;
    private GameObject hintButton;
    private Button resetButton;

    private int retryCount = 0;
    private int retryAdNum = 10;

    public float tileSpacing = 1.0f;

    public TileScriptableObject[] tileInfoObjects;

    private string[] levelInfo;
    private TileInfo[,] stageInfo; // 이차원 배열로 스테이지 정보 저장
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
        
        StartGame(Managers.UI.loadStageNum);
    }

    private void StartGame(int loadStageNum = 0)
    {
        admobManager = FindObjectOfType<AdmobManager>();

        // TODO: skip if current scene is Tutorial
        
        board = GameObject.Find("Tiles");
        
        retryButton = GameObject.Find("UI_Retry");
        if (retryButton != null)
        {
            retryButton.SetActive(false);
            retryButton.GetComponent<Button>().onClick.AddListener(Retry);
        }
        
        resetButton = GameObject.Find("ResetButton").GetComponent<Button>();

        nextButton = GameObject.Find("UI_Next");
        if (nextButton != null)
        {
            nextButton.SetActive(false);
            nextButton.GetComponent<Button>().onClick.AddListener(LoadNextStage);
        }

        hintButton = GameObject.Find("HintButton");
        if (hintButton != null)
        {
            hintButton.GetComponent<Button>().onClick.AddListener(ShowAdForHint);
        }
        
        currentStage = loadStageNum;

        currentSkinIndex = PlayerPrefs.GetInt("TILE_SKIN", 0);
        
        LoadStage(currentStage);

        Managers.Sound.Play("music", Definitions.Sound.Bgm);
    }

    private void LoadStage(int stage)
    {
        ColorManager.Instance.SetColor(stage);
        GameObject.Find("StageNumText").GetComponent<TextMeshProUGUI>().text = $"{stage}";
        LoadStageInfoFromCSV(stage);

        if (stageInfo != null)
        {
            CreatePuzzle();
        }
        else
        {
            Debug.LogError("CSV Loading Failed");
        }
    }

    private void LoadStageInfoFromCSV(int stage)
    {
        levelInfo ??= Resources.Load<TextAsset>("level_info").text.Split('\n');
        var values = levelInfo[stage].Split(',');
        
        maxClicks = int.Parse(values[1]);
        var row = int.Parse(values[2]);
        var column = int.Parse(values[3]);
        var type = values[4];
        var color = values[5];

        GameObject.Find("LimitTurnText").GetComponent<TextMeshProUGUI>().text = $"{maxClicks}";
        GameObject.Find("MoveTurnText").GetComponent<TextMeshProUGUI>().text = $"{currentClicks}";
        stageInfo = new TileInfo[row, column];
        width = stageInfo.GetLength(0);
        height = stageInfo.GetLength(1);

        for (var r = 0; r < row; r++)
        {
            for (var c = 0; c < column; c++)
            {
                var tileType = type[r * column + c];
                var tileColor = color[r * column + c];
                stageInfo[r, c] = new TileInfo(tileType, tileColor);
            }
        }
    }

    private float GetDistanceFromCenter(int x, int length)
    {
        return x - (length - 1) / 2.0f;
    }

    private void CreatePuzzle()
    {
        foreach (Transform child in board.transform) {
            Destroy(child.gameObject);
        }

        puzzleTiles = new PuzzleTile[width*height];
        RectTransform canvasRect = board.GetComponent<RectTransform>();

        for (var row = 0; row < width; row++)
        {
            for (var col = 0; col < height; col++)
            {
                var type = stageInfo[row, col].Type;
                var color = stageInfo[row, col].Color;

                var x = GetDistanceFromCenter(row, width) * tileSpacing;
                var y = -GetDistanceFromCenter(col, height) * tileSpacing;

                var tileObj = Instantiate(tilePrefab, canvasRect);
                tileObj.transform.localPosition = new Vector3(x, y, 0);

                var tile = tileObj.GetComponent<PuzzleTile>();
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

        if (retryCount > retryAdNum)
        {
            hintButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            hintButton.GetComponent<Button>().interactable = false;
        }
        
        var scale = 0.8f * Screen.width / 900;
        transform.localScale = new Vector3(scale, scale, 1);
    }
    
    private IEnumerator DelayedColorChange(Image img, Color c, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
        img.color = c;
    }
    
    private IEnumerator DelayedSpriteChange(Image img, Sprite s, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
        img.sprite = s;
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
        
        stageInfo[row, col].Color = (stageInfo[row, col].Color == 'W') ? 'B' : 'W';

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
        
        hintButton.GetComponent<Button>().interactable = false;
        currentClicks++;
        Managers.Sound.Play("flip2");
        GameObject.Find("MoveTurnText").GetComponent<TextMeshProUGUI>().text = $"{currentClicks}";
        if (CheckStageClear())
        {
            clickable = false;
            Invoke("SetNextButtonActive", 0.45f);
        }
        else if (currentClicks >= maxClicks)
        {
            // 클리어하지 못한 경우 처리
            clickable = false;
            Invoke("SetRetryButtonActive", 0.45f);
        }
    }

    private void SetNextButtonActive()
    {
        Managers.Sound.Play("stageclear");
        nextButton.transform.rotation = Quaternion.Euler(0, 270, 0);
        nextButton.transform.DORotate(new Vector3(0, 0, 0), 0.5f);
        resetButton.interactable = false;
        nextButton.SetActive(true);
    }
    
    private void SetRetryButtonActive()
    {
        retryButton.transform.rotation = Quaternion.Euler(0, 270, 0);
        retryButton.transform.DORotate(new Vector3(0, 0, 0), 0.5f);
        retryButton.SetActive(true);
    }

    public void Retry()
    {
        if (currentClicks > 0)
        {
            retryCount++;
            if (retryCount % retryAdNum == 0)
            {
                admobManager.ShowInterstitialAd();
            }
            Managers.Sound.Play("undo1");
            currentClicks = 0;
            LoadStage(currentStage);
            board.SetActive(true);
            retryButton.SetActive(false);
        
            clickable = true;
        }
    }
    
    private void LoadNextStage()
    {
        if (currentStage % 50 == 0)
        {
            PlayerPrefs.SetInt($"HAVE_SKIN_{currentStage/50}", 1);
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
        nextButton.SetActive(false);
        resetButton.interactable = true;
        
        clickable = true;
    }

    private void ShowAdForHint()
    {
        Managers.UI.ShowPopupUI<UI_ShowAdAskScreen>();
    }

    public void ShowHint()
    {
        var values = levelInfo[currentStage].Split(',');
        var hintX = int.Parse(values[6][2].ToString());
        var hintY = int.Parse(values[7][1].ToString());
        var s = puzzleTiles[hintX * width + hintY].AddComponent<Outline>();
        s.effectColor = Color.red;
        s.effectDistance = new Vector2(7f, 7f);
    }
}