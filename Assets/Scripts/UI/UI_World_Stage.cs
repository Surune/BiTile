using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_World_Stage : MonoBehaviour
{
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private Button button;
    [SerializeField] private Color lockedColor;
    private int chapter;
    private int stage;

    public void SetInfo(int chapter, int stage, int progressStage, int cleared)
    {
        this.chapter = chapter;
        this.stage = stage;
        stageText.text = stage.ToString();
        
        if (progressStage <= cleared)
        {
            button.onClick.AddListener(Accept);
        }
        else
        {
            GetComponentInChildren<Image>().color = lockedColor;
            stageText.color = Color.white;
            button.onClick.AddListener(Deny);
        }
    }

    private void Accept()
    {
        GameManager.Instance.SelectStage(chapter, stage);
        SceneManager.LoadScene(Definitions.GameSceneName);
    }

    private void Deny()
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Decline);
    }
}
