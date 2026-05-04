using UnityEngine;
using UnityEngine.UI;

public class UI_ShowAdAskScreen : UI_Popup
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
        GameManager.Instance.UI.ClosePopupUI();
        PuzzleManager.Instance.ShowHintWithRewardedAd();
    }

    private void OnNo()
    {
        GameManager.Instance.UI.ClosePopupUI();
    }
}
