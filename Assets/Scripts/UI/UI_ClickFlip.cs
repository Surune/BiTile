using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ClickFlip : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private Color flippedColor;

    private Image image;
    private Color defaultColor;
    private Vector3 defaultLocalEulerAngles;
    private bool isFlipped;

    private void Awake()
    {
        image = GetComponent<Image>();
        defaultColor = image.color;
        defaultLocalEulerAngles = transform.localEulerAngles;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isFlipped = !isFlipped;
        DOTween.Kill(this);

        DOTween.Sequence()
            .Append(transform.DOLocalRotate(defaultLocalEulerAngles + Vector3.up * (isFlipped ? 180f : 0f),
                    duration, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutCubic))
            .InsertCallback(duration * 0.5f, () => image.color = isFlipped ? flippedColor : defaultColor)
            .SetTarget(this)
            .SetLink(gameObject);
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
        isFlipped = false;
        image.color = defaultColor;
        transform.localEulerAngles = defaultLocalEulerAngles;
    }
}
