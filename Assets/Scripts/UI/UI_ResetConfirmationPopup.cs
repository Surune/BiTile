using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_ResetConfirmationPopup : MonoBehaviour
{
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action confirmAction;

    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Close);
    }

    public void Open(Action action)
    {
        confirmAction = action;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Close()
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
        gameObject.SetActive(false);
    }

    private void Confirm()
    {
        confirmAction();
        gameObject.SetActive(false);
    }
}
