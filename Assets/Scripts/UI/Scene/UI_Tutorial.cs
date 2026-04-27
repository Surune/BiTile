using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Tutorial : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private Button bgmButton;
    [SerializeField] private Button sfxButton;
    
    private void Awake()
    {
        exitButton.onClick.AddListener(OnExitButton);
        bgmButton.onClick.AddListener(OnBGMButton);
        sfxButton.onClick.AddListener(OnSFXButton);
        // TODO: OnSkinButton
    }

    private void OnExitButton()
    {
        SceneManager.LoadScene(Definitions.WorldSelectSceneName);
    }

    private void OnBGMButton()
    {
        Managers.Sound.ToggleBGMMute();
    }

    private void OnSFXButton()
    {
        Managers.Sound.ToggleSFXMute();
    }

    private void OnSkinButton(PointerEventData eventData)
    {
        Managers.UI.ShowPopupUI<UI_Popup>("UI_Skin");
    }
}