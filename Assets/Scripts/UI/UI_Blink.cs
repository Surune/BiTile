using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class UI_Blink : MonoBehaviour
{
    public TextMeshProUGUI textObject;
    public Color blinkColor = Color.white;
    public float blinkDuration = 1.0f;

    private Color originalColor;

    void Start()
    {
        originalColor = textObject.color;

        // Create a blink sequence
        Sequence blinkSequence = DOTween.Sequence();

        // Change the color back to the original color
        blinkSequence.Append(textObject.DOColor(blinkColor, blinkDuration));
        blinkSequence.Append(textObject.DOColor(originalColor, blinkDuration));
        blinkSequence.Play();
        blinkSequence.SetLoops(-1); // Loop infinitely
    }
}
