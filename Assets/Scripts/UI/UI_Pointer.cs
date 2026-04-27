using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_Pointer : MonoBehaviour
{
    [SerializeField] private float duration = 2.5f; // Duration of one fade cycle (fade in + fade out)
    [SerializeField] private float targetAlpha = 0f;
    private Image image;

    private void Start()
    {
        image = gameObject.GetComponent<Image>();

        LoopAlphaChange();
    }

    private void LoopAlphaChange()
    {
        // Fade to the target alpha, then back to fully visible, and repeat
        image.DOFade(targetAlpha, duration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
    }
}
