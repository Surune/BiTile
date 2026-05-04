using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Main : MonoBehaviour
{
    [SerializeField] private Image background;
    
    [SerializeField] private Button exitButton;
    [SerializeField] private Button bgmButton;
    [SerializeField] private Button sfxButton;
    
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text maxClicksText;
    [SerializeField] private TMP_Text currentClicksText;

    public void Init(int stage, int maxClicks, int currentClicks)
    {
        exitButton.onClick.AddListener(OnExitButton);
        bgmButton.onClick.AddListener(OnBGMButton);
        sfxButton.onClick.AddListener(OnSFXButton);
        // TODO: OnSkinButton

        background.color = GameManager.Instance.Color.GetBackgroundColor(stage);
        stageText.text = stage.ToString();
        UpdateClicks(maxClicks, currentClicks);
    }

    public void UpdateClicks(int maxClicks, int currentClicks)
    {
        maxClicksText.text = maxClicks.ToString();
        currentClicksText.text = currentClicks.ToString();
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
        GameManager.Instance.UI.ShowPopupUI<UI_Popup>("UI_Skin");
    }
}
