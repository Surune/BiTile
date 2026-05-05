using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_World_Stage : MonoBehaviour
{
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private Button button;
    [SerializeField] private Sprite clearedSprite;
    private int stageNum;

    public void SetInfo(int cur, int cleared)
    {
        stageNum = cur;
        stageText.text = stageNum.ToString();
        
        if (cur <= cleared)
        {
            GetComponentInChildren<Image>().overrideSprite = clearedSprite;
            GetComponentInChildren<TMP_Text>().color = Color.black;
            button.onClick.AddListener(Accept);
        }
        else
        {
            button.onClick.AddListener(Deny);
        }
    }

    private void Accept()
    {
        GameManager.Instance.StageSelection.LoadStageNum = stageNum;
        SceneManager.LoadScene(Definitions.GameSceneName);
    }

    private void Deny()
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Decline);
    }
}
