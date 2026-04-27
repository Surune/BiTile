using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_Pointer : MonoBehaviour
{
    private Image _image;
    public float duration = 2.5f; // Duration of one fade cycle (fade in + fade out)
    public float targetAlpha = 0f;
        
    void Start()
    {
        
        _image = gameObject.GetComponent<Image>();

        LoopAlphaChange();
    }

    void LoopAlphaChange()
    {
        // Fade to the target alpha, then back to fully visible, and repeat
        _image.DOFade(targetAlpha, duration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
    }
}
