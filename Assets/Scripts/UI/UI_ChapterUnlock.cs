using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ChapterUnlock : MonoBehaviour, IPointerClickHandler
{
    private const int PreviewLayer = 31;

    [SerializeField] private CanvasGroup overlay;
    [SerializeField] private CanvasGroup content;
    [SerializeField] private RectTransform previewRect;
    [SerializeField] private RawImage chapterPreview;
    [SerializeField] private TMP_Text chapterNameText;
    [SerializeField] private Image accent;
    [SerializeField] private Vector3 previewCameraPosition;
    [SerializeField] private Vector3 previewCameraRotation;
    [SerializeField] private float previewCameraSize = 3f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float revealDuration = 0.55f;
    [SerializeField] private float rotationDuration = 10f;

    private GameObject previewRoot;
    private RenderTexture previewTexture;
    private TaskCompletionSource<bool> touchCompletion;
    private Transform modelRoot;
    private bool isWaitingForTouch;
    private bool canContinue;

    public async Task Play(int chapter)
    {
        var chapterData = GameManager.Instance.Chapter.GetData(chapter);
        chapterNameText.text = $"{chapterData.RomanNumber}. {GameManager.Instance.Localization.Get(chapterData.NameLKey)}";
        accent.color = chapterData.BackgroundColor;
        modelRoot = CreatePreview(chapterData);
        touchCompletion = new TaskCompletionSource<bool>();
        isWaitingForTouch = true;

        overlay.alpha = 0f;
        content.alpha = 0f;
        previewRect.localScale = Vector3.one * 0.35f;
        previewRect.localRotation = Quaternion.Euler(0f, 0f, -8f);

        var intro = DOTween.Sequence();
        intro.Append(overlay.DOFade(1f, fadeDuration));
        intro.Append(content.DOFade(1f, revealDuration * 0.6f));
        intro.Join(previewRect.DOScale(1f, revealDuration).SetEase(Ease.OutBack));
        intro.Join(previewRect.DOLocalRotate(Vector3.zero, revealDuration).SetEase(Ease.OutCubic));

        await intro.AsyncWaitForCompletion();
        canContinue = true;
        await touchCompletion.Task;

        isWaitingForTouch = false;
        var outro = DOTween.Sequence();
        outro.Append(content.DOFade(0f, fadeDuration));
        await outro.AsyncWaitForCompletion();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canContinue)
        {
            return;
        }

        touchCompletion.TrySetResult(true);
    }

    private void Update()
    {
        if (isWaitingForTouch)
        {
            modelRoot.Rotate(Vector3.up * (360f / rotationDuration * Time.deltaTime), Space.Self);
        }
    }

    private Transform CreatePreview(ChapterData chapterData)
    {
        previewTexture = new RenderTexture(768, 512, 24, RenderTextureFormat.ARGB32);
        previewTexture.Create();
        chapterPreview.texture = previewTexture;

        previewRoot = new GameObject("ChapterUnlockPreview");
        previewRoot.transform.position = Vector3.one * 10000f;

        var cameraObject = new GameObject("Camera");
        cameraObject.transform.SetParent(previewRoot.transform, false);
        cameraObject.transform.localPosition = previewCameraPosition;
        cameraObject.transform.localRotation = Quaternion.Euler(previewCameraRotation);
        var previewCamera = cameraObject.AddComponent<Camera>();
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = Color.clear;
        previewCamera.orthographic = true;
        previewCamera.orthographicSize = previewCameraSize;
        previewCamera.cullingMask = 1 << PreviewLayer;
        previewCamera.targetTexture = previewTexture;

        var modelObject = new GameObject("Model");
        modelObject.transform.SetParent(previewRoot.transform, false);
        Instantiate(chapterData.TileModel, modelObject.transform);
        var numberModel = Instantiate(chapterData.NumberModel, modelObject.transform);
        numberModel.transform.localScale = Vector3.one * 0.9f;
        SetLayerRecursively(previewRoot.transform);
        return modelObject.transform;
    }

    private static void SetLayerRecursively(Transform target)
    {
        target.gameObject.layer = PreviewLayer;
        foreach (Transform child in target)
        {
            SetLayerRecursively(child);
        }
    }

    private void OnDestroy()
    {
        Destroy(previewRoot);
        previewTexture.Release();
        Destroy(previewTexture);
    }
}
