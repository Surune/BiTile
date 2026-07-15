using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_World_Stage : MonoBehaviour
{
    [SerializeField] private Image tileImage;
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private Button button;
    [SerializeField] private Color lockedColor;
    [SerializeField] private Image starImage;
    [SerializeField] private Image starOutlineImage;
    
    private int chapter;
    private int stage;

    public void SetInfo(int chapter, int stage, int progressStage, int cleared, bool hasStar, Color starColor)
    {
        this.chapter = chapter;
        this.stage = stage;
        starImage.color = hasStar ? starColor : lockedColor;
        stageText.text = stage.ToString();
        
        if (progressStage <= cleared)
        {
            tileImage.color = Color.white;
            button.onClick.AddListener(Accept);
        }
        else
        {
            tileImage.color = lockedColor;
            starImage.gameObject.SetActive(false);
            starOutlineImage.gameObject.SetActive(false);
            stageText.color = Color.white;
            button.interactable = false;
            button.onClick.AddListener(Deny);
        }
    }

    private void Accept()
    {
        GameManager.Instance.SetStage(chapter, stage);
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
        SceneManager.LoadScene(Definitions.GameSceneName);
    }

    private void Deny()
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Decline);
    }
}
