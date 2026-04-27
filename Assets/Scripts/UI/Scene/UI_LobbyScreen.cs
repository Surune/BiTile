using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_LobbyScreen : MonoBehaviour
{
    [SerializeField] private Button startSceneButton;
    [SerializeField] private Button privacyPolicyButton;

    private void Awake()
    {
        startSceneButton.onClick.AddListener(OnWorldSelect);
        privacyPolicyButton.onClick.AddListener(OnPrivacyPolicy);

        GameManager.Instance.Sound.PlayBGM(Definitions.SoundType.Bgm);
    }

    private void OnWorldSelect()
    {
        SceneManager.LoadScene(Definitions.StageSelectSceneName);
    }

    private void OnPrivacyPolicy()
    {
        Application.OpenURL("https://sites.google.com/view/bitile-privacy-policy");
    }
}
