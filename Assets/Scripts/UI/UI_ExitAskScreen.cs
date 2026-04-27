using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_ExitAskScreen : UI_Popup
{
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    public override void Init()
    {
        base.Init();

        yesButton.onClick.AddListener(OnYes);
        noButton.onClick.AddListener(OnNo);
    }

    private void OnYes()
    {
        SceneManager.LoadScene(Definitions.LobbySceneName);
    }

    private void OnNo()
    {
        Managers.UI.ClosePopupUI();
    }
}
