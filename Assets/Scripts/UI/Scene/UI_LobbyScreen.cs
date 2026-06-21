using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_LobbyScreen : MonoBehaviour
{
    [SerializeField] private Button startSceneButton;

    private void Awake()
    {
        startSceneButton.onClick.AddListener(OnWorldSelect);

        GameManager.Instance.Sound.PlayBGM(Definitions.SoundType.Bgm);
    }

    private void OnWorldSelect()
    {
        SceneManager.LoadScene(Definitions.ChapterSelectSceneName);
    }
}
