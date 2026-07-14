using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainGame : MonoBehaviour
{
    private const string StarCountFormat = "<sprite index=0> {0}/{1}";

    [SerializeField] private Button exitButton;
    [SerializeField] private Button undoButton;
    
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private Color starExceededColor;
    [SerializeField] private UI_DiagonalFlowBackground diagonalFlowBackground;
    
    private bool isExiting;
    
    private void Awake()
    {
        exitButton.onClick.AddListener(OnExitButton);
    }

    public void Init(int chapter, int stage, int maxClicks, int currentClicks, Definitions.LKey tutorialLkey, Sprite[] backgroundSprites)
    {
        var chapterData = GameManager.Instance.Chapter.GetData(chapter);
        var chapterName = GameManager.Instance.Localization.Get(chapterData.NameLKey);
        stageText.text = $"{chapterData.RomanNumber}. {chapterName} - {stage}";
        tutorialText.text = GameManager.Instance.Localization.Get(tutorialLkey);
        diagonalFlowBackground.SetSprites(backgroundSprites);
        UpdateClicks(currentClicks, maxClicks);
    }

    public void UpdateClicks(int currentClicks, int maxClicks)
    {
        counterText.text = string.Format(StarCountFormat, currentClicks, maxClicks);
        counterText.color = currentClicks > maxClicks ? starExceededColor : Color.white;
    }

    private void OnExitButton()
    {
        if (isExiting)
        {
            return;
        }

        isExiting = true;
        UI_Lobby.OpenStageSelectOnAwake = true;
        SceneManager.LoadScene(Definitions.LobbySceneName);
    }
}
