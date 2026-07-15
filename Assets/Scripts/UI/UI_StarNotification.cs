using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_StarNotification : MonoBehaviour
{
    [SerializeField] private Image dimOverlay;
    [SerializeField] private TMP_Text starText;
    [SerializeField] private RectTransform nextButtonTarget;
    [SerializeField] private float dimAlpha = 0.9f;
    [SerializeField] private float dimFadeDuration = 0.1f;
    [SerializeField] private float stampStartScale = 3.2f;
    [SerializeField] private float stampDuration = 0.1f;
    [SerializeField] private float impactScale = 0.8f;
    [SerializeField] private float reboundScale = 1.08f;
    [SerializeField] private float reboundDuration = 0.05f;
    [SerializeField] private float settleDuration = 0.1f;
    [SerializeField] private float holdDuration = 1f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private float exitMoveDuration = 0.25f;
    [SerializeField] private float exitScale = 0.5f;
    [SerializeField] private float targetOffset = 150f;

    public void Play()
    {
        DOTween.Kill(this);
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
        sequence.Join(starText.DOFade(1f, dimFadeDuration * 0.5f));
        sequence.Join(starText.rectTransform.DOScale(Vector3.one * impactScale, stampDuration).SetEase(Ease.InQuart));
        sequence.AppendCallback(() => GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Star));
        sequence.Append(starText.rectTransform.DOScale(Vector3.one * reboundScale, reboundDuration).SetEase(Ease.OutQuad));
        sequence.Append(starText.rectTransform.DOScale(Vector3.one, settleDuration).SetEase(Ease.OutBack));
        sequence.AppendInterval(holdDuration);
        var targetPosition = nextButtonTarget.TransformPoint(Vector3.up * targetOffset);
        sequence.Append(dimOverlay.DOFade(0f, fadeOutDuration));
        sequence.Join(starText.rectTransform.DOMove(targetPosition, exitMoveDuration).SetEase(Ease.Linear));
        sequence.Join(starText.rectTransform.DOScale(Vector3.one * exitScale, exitMoveDuration).SetEase(Ease.Linear));
    }

    public void Hide()
    {
        DOTween.Kill(this);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
