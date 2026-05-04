using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkinPanel : MonoBehaviour
{
    [SerializeField] private Image skinImage;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text unlockText;
    private SkinScriptableObject skin;
    private int index;
    
    public void Init(SkinScriptableObject skinSO, int skinIndex)
    {
        skin = skinSO;
        index = skinIndex;
        
        skinImage.sprite = skin.skinImage;
        
        button.interactable = PlayerPrefs.GetInt($"STAGE", 0) >= skin.unlockStage;
        button.onClick.AddListener(SetCurrentSkin);    
        
        unlockText.text = $"STAGE {skin.unlockStage}";
    }
    
    private void SetCurrentSkin()
    {
        PlayerPrefs.SetInt("TILE_SKIN", index);
        PlayerPrefs.Save();
        GameManager.Instance.Sound.Play("flip4");
    }
}