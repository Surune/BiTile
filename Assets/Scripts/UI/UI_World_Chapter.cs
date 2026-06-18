using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_World_Chapter : MonoBehaviour
{
    [SerializeField] private TMP_Text chapterText;
    [SerializeField] private Button button;
    private UI_StageSelect stageSelect;
    private int chapter;

    public void SetInfo(UI_StageSelect stageSelect, int chapter, bool isUnlocked)
    {
        this.stageSelect = stageSelect;
        this.chapter = chapter;
        chapterText.text = chapter.ToString();
        button.interactable = isUnlocked;
        
        button.onClick.AddListener(SelectChapter);
    }

    private void SelectChapter()
    {
        stageSelect.SelectChapter(chapter);
    }
}
