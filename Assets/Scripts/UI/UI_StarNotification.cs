using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_StarNotification : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private Image dimOverlay;
    [SerializeField] private TMP_Text starText;
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private float dimAlpha = 1f;
    [SerializeField] private float dimFadeDuration = 0.1f;
    [SerializeField] private float stampStartScale = 5f;
    [SerializeField] private float stampDuration = 0.1f;
    [SerializeField] private float impactScale = 0.8f;
    [SerializeField] private float reboundScale = 1.08f;
    [SerializeField] private float reboundDuration = 0.05f;
    [SerializeField] private float settleDuration = 0.1f;
    [SerializeField] private float holdDuration = 1.75f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private float exitMoveDuration = 0.25f;
    [SerializeField] private float exitScale = 0.7f;
    private Vector3 originalAnchoredPos;

    private void Awake()
    {
        originalAnchoredPos = rect.anchoredPosition;
    }

    public void Play()
    {
        DOTween.Kill(this);
        rect.anchoredPosition = originalAnchoredPos;
        transform.localScale = Vector3.one;
        gameObject.SetActive(true);

        dimOverlay.color = Color.clear;
        starText.alpha = 0f;
        starText.rectTransform.anchoredPosition = Vector2.zero;
        starText.rectTransform.localScale = Vector3.one * stampStartScale;
        starText.rectTransform.localRotation = Quaternion.identity;

        var sequence = DOTween.Sequence()
            .SetTarget(this)
            .SetUpdate(true)
            .SetLink(gameObject);

        sequence.Append(dimOverlay.DOFade(dimAlpha, dimFadeDuration));
        sequence.Join(counterText.DOFade(0f, dimFadeDuration * 0.5f));
        sequence.Join(starText.DOFade(1f, dimFadeDuration * 0.5f));
        sequence.Join(starText.rectTransform.DOScale(Vector3.one * impactScale, stampDuration).SetEase(Ease.InQuad));
        sequence.AppendCallback(() => GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Star));
        sequence.Append(starText.rectTransform.DOScale(Vector3.one * reboundScale, reboundDuration).SetEase(Ease.InQuad));
        sequence.Append(starText.rectTransform.DOScale(Vector3.one, settleDuration).SetEase(Ease.InQuad));
        sequence.AppendInterval(holdDuration);
        sequence.Append(dimOverlay.DOFade(0f, fadeOutDuration).SetEase(Ease.OutQuad));
        sequence.Join(transform.DOMove(counterText.transform.position, exitMoveDuration).SetEase(Ease.InQuad));
        sequence.Join(transform.DOScale(Vector3.one * exitScale, exitMoveDuration).SetEase(Ease.InQuad));
    }

    public void Hide()
    {
        DOTween.Kill(this);
        counterText.color = Color.white;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
