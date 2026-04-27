using UnityEngine;
using DG.Tweening;
using TMPro;

public class UI_Blink : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textObject;
    [SerializeField] private Color blinkColor = Color.white;
    [SerializeField] private float blinkDuration = 1.0f;
    private Color originalColor;

    private void Awake()
    {
        originalColor = textObject.color;

        // Create a blink sequence
        var blinkSequence = DOTween.Sequence();

        // Change the color back to the original color
        blinkSequence.Append(textObject.DOColor(blinkColor, blinkDuration));
        blinkSequence.Append(textObject.DOColor(originalColor, blinkDuration));
        blinkSequence.Play();
        blinkSequence.SetLoops(-1); // Loop infinitely
    }
}
