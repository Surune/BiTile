using UnityEngine;

public class HintTile : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private float hueSpeed = 1f;

    private Material material;
    private float hue;
    private bool isVisible;

    private void Awake()
    {
        material = meshRenderer.material;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SetHue(hue);
        isVisible = true;
    }

    public void Hide()
    {
        if (isVisible)
        {
            isVisible = false;
        }

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isVisible)
        {
            SetHue(hue + Time.deltaTime * hueSpeed);
        }
    }

    private void SetHue(float value)
    {
        hue = Mathf.Repeat(value, 1f);
        var hintColor = Color.HSVToRGB(hue, 1f, 1f);
        material.color = hintColor;
    }
}
