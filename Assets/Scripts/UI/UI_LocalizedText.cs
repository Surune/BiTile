using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class UI_LocalizedText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Definitions.LKey lkey;

    private void OnValidate()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        GameManager.Instance.Localization.LocaleChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        GameManager.Instance.Localization.LocaleChanged -= Refresh;
    }

    private void Refresh()
    {
        text.text = GameManager.Instance.Localization.Get(lkey);
    }
}
