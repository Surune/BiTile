using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class UI_LocalizedText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Definitions.LKey lkey;
    private Localization Localization => GameManager.Instance.Localization;

    private void OnValidate()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        Localization.LocaleChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        Localization.LocaleChanged -= Refresh;
    }

    private void Refresh()
    {
        text.text = Localization.Get(lkey);
    }
}
