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
    
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text maxClicksText;
    [SerializeField] private TMP_Text currentClicksText;

    [SerializeField] private UI_SkinPopup skinPopupPrefab;
    
    private void Awake()
    {
        exitButton.onClick.AddListener(OnExitButton);
        bgmButton.onClick.AddListener(OnBGMButton);
        sfxButton.onClick.AddListener(OnSFXButton);
        skinButton.onClick.AddListener(OnSkinButton);
    }

    public void Init(int stage, int maxClicks, int currentClicks)
    {
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
        Instantiate(skinPopupPrefab, transform);
    }
}
