using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class UI_ButtonHoverSize : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float HoverSizeScale = 1.1f;
    [SerializeField] private float Duration = 0.1f;

    private Button button;
    private Vector3 defaultScale;
    private Tween scaleTween;

    private void Awake()
    {
        button = GetComponent<Button>();
        defaultScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable)
        {
            ResetImmediately();
            return;
        }

        Play(defaultScale * HoverSizeScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Play(defaultScale);
    }
    
    private void Play(Vector3 scale)
    {
        scaleTween?.Kill();
        scaleTween = transform.DOScale(scale, Duration)
            .SetEase(Ease.OutCubic)
            .SetTarget(this)
            .SetLink(gameObject);
    }

    private void ResetImmediately()
    {
        scaleTween?.Kill();
        transform.localScale = defaultScale;
    }

    private void OnDisable()
    {
        ResetImmediately();
    }

    private void OnDestroy()
    {
        scaleTween?.Kill();
    }
}
