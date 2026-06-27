using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Option : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TMP_Text bgmValue;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxValue;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        bgmSlider.onValueChanged.AddListener(OnBgmSlider);
        sfxSlider.onValueChanged.AddListener(OnSfxSlider);
    }

    public void Open()
    {
        bgmSlider.SetValueWithoutNotify(GameManager.Instance.Sound.BgmVolume);
        sfxSlider.SetValueWithoutNotify(GameManager.Instance.Sound.SfxVolume);
        UpdateBgmValue(GameManager.Instance.Sound.BgmVolume);
        UpdateSfxValue(GameManager.Instance.Sound.SfxVolume);
        gameObject.SetActive(true);
    }

    private void OnBgmSlider(float value)
    {
        GameManager.Instance.Sound.SetBgmVolume(value);
        UpdateBgmValue(value);
    }

    private void OnSfxSlider(float value)
    {
        GameManager.Instance.Sound.SetSfxVolume(value);
        UpdateSfxValue(value);
    }

    private void UpdateBgmValue(float value)
    {
        bgmValue.text = Mathf.RoundToInt(value * 100f).ToString();
    }

    private void UpdateSfxValue(float value)
    {
        sfxValue.text = Mathf.RoundToInt(value * 100f).ToString();
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}
