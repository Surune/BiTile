using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_LogoCrtEffect : MonoBehaviour
{
    [SerializeField] private Image logoImage;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Shader crtShader;
    [SerializeField] private float chromaticOffset = 5f;
    [SerializeField] private float chromaticAlpha = 0.5f;
    [SerializeField] private float curvatureStrength = 0.18f;
    [SerializeField] private float scanlineDensity = 180f;
    [SerializeField] private float scanlineStrength = 1f;
    [SerializeField] private float fineLineStrength = 0.8f;
    [SerializeField] private float vignetteStrength = 0.8f;
    [SerializeField] private float flickerStrength = 0.1f;
    [SerializeField] private float displayDelay = 0.2f;
    [SerializeField] private float powerOnDuration = 0.1f;
    [SerializeField] private float powerOffDelay = 2f;
    [SerializeField] private float powerOffCompressDuration = 0.1f;
    [SerializeField] private float powerOffFlickerDuration = 0.18f;
    [SerializeField] private float powerFlickerStrength = 0.18f;

    private Material crtMaterial;
    private Image blueImage;
    private Image redImage;
    private RawImage overlayImage;
    private Image topShutter;
    private Image bottomShutter;
    private Image powerLine;
    private float elapsedTime;
    private bool isSceneLoading;

    private void Awake()
    {
        logoImage.raycastTarget = false;

        blueImage = CreateChromaticImage("CRT Blue", Color.blue, Vector2.left * chromaticOffset);
        redImage = CreateChromaticImage("CRT Red", Color.red, Vector2.right * chromaticOffset);
        logoImage.transform.SetSiblingIndex(2);

        CreateOverlay();
        CreatePowerShutter();
        ApplyPower(0f, 1f, 0f, 0f, 1f);
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        var sequenceTime = elapsedTime - displayDelay;
        if (sequenceTime < 0f)
        {
            ApplyPower(0f, 1f, 0f, 0f, 1f);
            return;
        }

        if (sequenceTime < powerOnDuration)
        {
            var progress = Mathf.Clamp01(sequenceTime / powerOnDuration);
            var easedProgress = Mathf.Clamp01(EaseOutBack(progress));
            var flash = Mathf.Abs(Mathf.Sin(progress * Mathf.PI * 6f)) * (1f - progress) * 0.75f;
            var powerOnFlashScaleY = Mathf.Max(0.02f, easedProgress);
            ApplyPower(easedProgress, 1f - EaseOutCubic(progress), easedProgress, flash, powerOnFlashScaleY);
            return;
        }

        if (sequenceTime < powerOffDelay)
        {
            ApplyPower(1f, 0f, 1f, 0f, 1f);
            return;
        }

        var offElapsedTime = sequenceTime - powerOffDelay;
        if (offElapsedTime < powerOffCompressDuration)
        {
            var compressProgress = EaseInCubic(Mathf.Clamp01(offElapsedTime / powerOffCompressDuration));
            ApplyPower(1f, compressProgress, 1f - compressProgress * 0.98f, 0f, 1f);
            return;
        }

        var flickerElapsedTime = offElapsedTime - powerOffCompressDuration;
        if (flickerElapsedTime < powerOffFlickerDuration)
        {
            var flickerProgress = Mathf.Clamp01(flickerElapsedTime / powerOffFlickerDuration);
            var flicker = Mathf.Abs(Mathf.Sin(flickerProgress * Mathf.PI * 7f)) * (1f - flickerProgress);
            var flashScaleY = 1f - EaseOutCubic(flickerProgress) * 0.98f;
            ApplyPower(flicker, 1f, 0.02f, flicker * 0.95f, flashScaleY);
            return;
        }

        ApplyPower(0f, 1f, 0.02f, 0f, 0.02f);

        if (flickerElapsedTime < powerOffFlickerDuration + displayDelay || isSceneLoading)
        {
            return;
        }

        isSceneLoading = true;
        SceneManager.LoadScene(Definitions.LobbySceneName);
    }

    private Image CreateChromaticImage(string layerName, Color color, Vector2 offset)
    {
        color.a = chromaticAlpha;

        var layer = Instantiate(logoImage, logoImage.transform.parent);
        layer.name = layerName;
        layer.color = color;
        layer.raycastTarget = false;
        layer.material = logoImage.material;

        var rectTransform = (RectTransform)layer.transform;
        rectTransform.anchoredPosition = ((RectTransform)logoImage.transform).anchoredPosition + offset;
        rectTransform.SetSiblingIndex(logoImage.transform.GetSiblingIndex());
        return layer;
    }

    private void CreateOverlay()
    {
        crtMaterial = new Material(crtShader);
        crtMaterial.SetFloat("_ScanlineDensity", scanlineDensity);
        crtMaterial.SetFloat("_ScanlineStrength", scanlineStrength);
        crtMaterial.SetFloat("_FineLineStrength", fineLineStrength);
        crtMaterial.SetFloat("_VignetteStrength", vignetteStrength);
        crtMaterial.SetFloat("_FlickerStrength", flickerStrength);
        crtMaterial.SetFloat("_CurvatureStrength", curvatureStrength);

        var overlay = new GameObject("CRT Overlay");
        overlay.layer = canvas.gameObject.layer;
        overlay.transform.SetParent(canvas.transform, false);

        var rectTransform = overlay.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        overlayImage = overlay.AddComponent<RawImage>();
        overlayImage.raycastTarget = false;
        overlayImage.color = Color.white;
        overlayImage.material = crtMaterial;

        overlay.transform.SetAsLastSibling();
    }

    private void CreatePowerShutter()
    {
        topShutter = CreateShutter("CRT Top Shutter", 0.5f, 1f, 1f);
        bottomShutter = CreateShutter("CRT Bottom Shutter", 0f, 0.5f, 0f);

        var flash = new GameObject("CRT Power Flash");
        flash.layer = canvas.gameObject.layer;
        flash.transform.SetParent(canvas.transform, false);

        var rectTransform = flash.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        powerLine = flash.AddComponent<Image>();
        powerLine.raycastTarget = false;
        powerLine.color = Color.white;
        flash.transform.SetAsLastSibling();
    }

    private Image CreateShutter(string layerName, float anchorMinY, float anchorMaxY, float pivotY)
    {
        var shutter = new GameObject(layerName);
        shutter.layer = canvas.gameObject.layer;
        shutter.transform.SetParent(canvas.transform, false);

        var rectTransform = shutter.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.up * anchorMinY;
        rectTransform.anchorMax = Vector2.right + Vector2.up * anchorMaxY;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        var pivot = rectTransform.pivot;
        pivot.y = pivotY;
        rectTransform.pivot = pivot;

        var image = shutter.AddComponent<Image>();
        image.raycastTarget = false;
        image.color = Color.black;
        shutter.transform.SetAsLastSibling();
        return image;
    }

    private void ApplyPower(float contentPower, float shutterPower, float contentScaleY, float flashAlpha, float flashScaleY)
    {
        var noise = Mathf.Sin(Time.time * 74f) * Mathf.Sin(Time.time * 31f) * powerFlickerStrength;
        var flickeredContentPower = Mathf.Clamp01(contentPower + noise * (1f - contentPower));
        SetImageAlpha(logoImage, flickeredContentPower);
        SetImageAlpha(blueImage, chromaticAlpha * flickeredContentPower);
        SetImageAlpha(redImage, chromaticAlpha * flickeredContentPower);
        SetRawImageAlpha(overlayImage, Mathf.Clamp01(0.35f + flickeredContentPower * 0.65f));
        SetImageAlpha(topShutter, shutterPower);
        SetImageAlpha(bottomShutter, shutterPower);
        SetImageAlpha(powerLine, flashAlpha);
        SetShutterScale(topShutter, shutterPower);
        SetShutterScale(bottomShutter, shutterPower);
        SetContentScaleY(contentScaleY);
        SetScaleY(powerLine, flashScaleY);
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        var color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void SetRawImageAlpha(RawImage image, float alpha)
    {
        var color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void SetShutterScale(Image image, float shutterPower)
    {
        var scale = image.transform.localScale;
        scale.y = shutterPower;
        image.transform.localScale = scale;
    }

    private void SetContentScaleY(float scaleY)
    {
        SetScaleY(logoImage, scaleY);
        SetScaleY(blueImage, scaleY);
        SetScaleY(redImage, scaleY);
    }

    private void SetScaleY(Image image, float scaleY)
    {
        var scale = image.transform.localScale;
        scale.y = scaleY;
        image.transform.localScale = scale;
    }

    private float EaseOutCubic(float value)
    {
        return 1f - Mathf.Pow(1f - value, 3f);
    }

    private float EaseInCubic(float value)
    {
        return value * value * value;
    }

    private float EaseOutBack(float value)
    {
        var overshoot = 1.35f;
        var shiftedValue = value - 1f;
        return 1f + shiftedValue * shiftedValue * ((overshoot + 1f) * shiftedValue + overshoot);
    }

    private void OnDestroy()
    {
        Destroy(crtMaterial);
    }
}
