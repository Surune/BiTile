using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Main : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private Button bgmButton;
    [SerializeField] private Button sfxButton;
    [SerializeField] private Button skinButton;
    [SerializeField] private Button undoButton;
    
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text clicksText;
    [SerializeField] private TMP_Text tutorialText;

    [SerializeField] private UI_SkinPopup skinPopupPrefab;

    public Button UndoButton => undoButton;
    
    private void Awake()
    {
        exitButton.onClick.AddListener(OnExitButton);
        bgmButton.onClick.AddListener(OnBGMButton);
        sfxButton.onClick.AddListener(OnSFXButton);
        skinButton.onClick.AddListener(OnSkinButton);
    }

    public void Init(int stage, int maxClicks, int currentClicks, string tutorialLkey)
    {
        stageText.text = stage.ToString();
        tutorialText.text = GameManager.Instance.Localization.Get(tutorialLkey);
        UpdateClicks(maxClicks, currentClicks);
    }

    public void UpdateClicks(int maxClicks, int currentClicks)
    {
        clicksText.text = $"{currentClicks}/{maxClicks}";
    }

    private void OnExitButton()
    {
        SceneManager.LoadScene(Definitions.StageSelectSceneName);
    }

    private void OnBGMButton()
    {
        GameManager.Instance.Sound.ToggleBGMMute();
    }

    private void OnSFXButton()
    {
        GameManager.Instance.Sound.ToggleSFXMute();
    }
    
    private void OnSkinButton()
    {
        Instantiate(skinPopupPrefab, transform);
    }
}
