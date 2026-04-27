using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_WorldSelect : MonoBehaviour
{
    [SerializeField] private Button worldButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    
    [SerializeField] private TMP_Text worldText;
    [SerializeField] private TMP_Text stageText;

    private const int tutorialWorldNum = 0;
    private int worldNum = 1;
    private int worldMax = 6;

    private void Awake()
    {
        worldButton.onClick.AddListener(OnWorld);
        leftButton.onClick.AddListener(OnLeftArrowButton);
        rightButton.onClick.AddListener(OnRightArrowButton);

        worldNum = PlayerPrefs.GetInt("WORLD", 0);
        SetContents();
    }

    private void OnWorld()
    {
        if (worldNum == tutorialWorldNum)
        {
            SceneManager.LoadScene(Definitions.TutorialSceneName);
        }
        else
        {
            Managers.UI.ShowPopupUI<UI_StageSelect>();
            Managers.UI.justClickedWorld = worldNum;
        }
    }
    
    private void OnLeftArrowButton()
    {
        worldNum--;
        if (worldNum < 0)
        {
            worldNum = worldMax;
        }
        PlayerPrefs.SetInt("WORLD", worldNum);
        SetContents();
    }

    private void OnRightArrowButton()
    {
        worldNum++;
        if (worldNum > worldMax)
        {
            worldNum = 0;
        }
        PlayerPrefs.SetInt("WORLD", worldNum);
        SetContents();
    }

    private void SetContents()
    {
        if (worldNum == tutorialWorldNum)
        {
            worldText.text = $"Tutorial";
            worldText.DOColor(Color.black, 0.5f);
            stageText.text = $"HOW TO PLAY";
            stageText.DOColor(Color.white, 0.5f);
        }
        else
        {
            worldText.text = $"World {worldNum}";
            worldText.DOColor(Color.white, 0.5f);
            stageText.text = $"STAGE {1 + 35 * (worldNum - 1)}~{(35 * worldNum)}";
            stageText.DOColor(Colorset.tileColors[worldNum - 1], 0.5f);
        }
    }
}
