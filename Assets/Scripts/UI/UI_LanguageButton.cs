using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_LanguageButton : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button button;
    private string locale;

    public void Init(int index)
    {
        locale = Localization.SupportedLocales[index];
        text.text = Localization.SupportedLocaleLabels[index];
        button.onClick.AddListener(ChangeLocale);
        GameManager.Instance.Localization.LocaleChanged += RefreshButton;
        RefreshButton();
    }

    private void OnDestroy()
    {
        GameManager.Instance.Localization.LocaleChanged -= RefreshButton;
    }

    private void ChangeLocale()
    {
        GameManager.Instance.Localization.SetLocale(locale);
        RefreshButton();
    }

    private void RefreshButton()
    {
        button.interactable = locale != GameManager.Instance.Localization.CurrentLocale;
    }
}
