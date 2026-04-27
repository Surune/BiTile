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
        Managers.UI.ClosePopupUI();

        FindObjectOfType<AdmobManager>().ShowRewardedAd();
    }

    private void OnNo()
    {
        Managers.UI.ClosePopupUI();
    }
}
