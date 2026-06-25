using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_LobbyScreen : MonoBehaviour
{
    [SerializeField] private Button resetButton;
    [SerializeField] private Button cheatButton;
    [SerializeField] private Button startButton;

    private void Awake()
    {
        resetButton.onClick.AddListener(SaveManager.Reset);
        cheatButton.onClick.AddListener(SaveManager.CompleteAllStages);
        startButton.onClick.AddListener(OnWorldSelect);

        GameManager.Instance.Sound.PlayBGM(Definitions.SoundType.Bgm);
    }

    private void OnWorldSelect()
    {
        SceneManager.LoadScene(Definitions.ChapterSelectSceneName);
    }
}
