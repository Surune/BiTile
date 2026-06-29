using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Main : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private Button bgmButton;
    [SerializeField] private Button sfxButton;
    [SerializeField] private Button undoButton;
    
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private UI_Counter counterPrefab;
    [SerializeField] private Transform counterParent;

    public Button UndoButton => undoButton;
    
    private readonly List<UI_Counter> counters = new List<UI_Counter>();
    private bool isExiting;
    
    private void Awake()
    {
        exitButton.onClick.AddListener(OnExitButton);
        bgmButton.onClick.AddListener(OnBGMButton);
        sfxButton.onClick.AddListener(OnSFXButton);
    }

    public void Init(int stage, int maxClicks, int currentClicks, Definitions.LKey tutorialLkey)
    {
        stageText.text = stage.ToString();
        tutorialText.text = GameManager.Instance.Localization.Get(tutorialLkey);
        SetupCounters(maxClicks);
        UpdateClicks(currentClicks);
    }

    public void UpdateClicks(int currentClicks)
    {
        for (int i = 0; i < counters.Count; i++)
        {
            if (i < currentClicks)
            {
                counters[i].Use();
                continue;
            }
            
            counters[i].Unuse();
        }
    }

    private void SetupCounters(int maxClicks)
    {
        while (counters.Count < maxClicks)
        {
            var counter = Instantiate(counterPrefab, counterParent);
            counters.Add(counter);
        }

        while (counters.Count > maxClicks)
        {
            var lastIndex = counters.Count - 1;
            Destroy(counters[lastIndex].gameObject);
            counters.RemoveAt(lastIndex);
        }

        foreach (var counter in counters)
        {
            counter.Unuse();
        }
    }

    private void OnExitButton()
    {
        if (isExiting)
        {
            return;
        }

        isExiting = true;
        UI_LobbyScreen.OpenStageSelectOnAwake = true;
        SceneManager.LoadScene(Definitions.LobbySceneName);
    }

    private void OnBGMButton()
    {
        GameManager.Instance.Sound.ToggleBGMMute();
    }

    private void OnSFXButton()
    {
        GameManager.Instance.Sound.ToggleSFXMute();
    }
}
