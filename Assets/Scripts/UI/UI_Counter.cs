using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_Counter : MonoBehaviour
{
    [SerializeField] private Image innerCircle;
    [SerializeField] private float UseScaleDuration = 0.25f;
    
    private bool isUsed;

    public void Use()
    {
        if (isUsed)
        {
            return;
        }

        isUsed = true;
        innerCircle.transform.DOKill();
        innerCircle.transform.localScale = Vector3.zero;
        innerCircle.transform.DOScale(Vector3.one, UseScaleDuration);
    }

    public void Unuse()
    {
        isUsed = false;
        innerCircle.transform.DOKill();
        innerCircle.transform.DOScale(Vector3.zero, UseScaleDuration);
    }
}
