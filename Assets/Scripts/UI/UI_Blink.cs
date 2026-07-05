using UnityEngine;
using DG.Tweening;
using TMPro;

public class UI_Blink : MonoBehaviour
{
    [SerializeField] private TMP_Text textObject;
    [SerializeField] private Color blinkColor = Color.white;
    [SerializeField] private float blinkDuration = 1.0f;
    private Color originalColor;

    private void Awake()
    {
        originalColor = textObject.color;
        
        var blinkSequence = DOTween.Sequence();
        blinkSequence.Append(textObject.DOColor(blinkColor, blinkDuration));
        blinkSequence.Append(textObject.DOColor(originalColor, blinkDuration));
        blinkSequence.Play();
        blinkSequence.SetLoops(-1);
    }
}
