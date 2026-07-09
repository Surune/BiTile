using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UI_World_Chapter : MonoBehaviour, IPointerClickHandler
{
    private const string StarCountFormat = "<sprite index=0> {0}/{1}";

    [Header("Input")]
    [SerializeField] private InputActionReference rightClick;
    
    [Header("Visual")]
    [SerializeField] private TMP_Text starCountText;
    [SerializeField] private float groundRotateSpeed = 5f;
    [SerializeField] private float fastGroundRotateSpeed = 180f;
    [SerializeField] private float numberScale = 0.9f;
    [SerializeField] private float numberFloatSpeed = 1f;
    [SerializeField] private float numberFloatHeight = 0.1f;
    [SerializeField] private GameObject lockedNumber;
    [SerializeField] private Color lockedColor;
    [SerializeField] private Material completedGlowMaterial;
    [SerializeField] private float completedGlowFlowSpeed = 0.45f;

    private Vector3 numberModelDefaultLocalPosition;
    private UI_ChapterCarousel chapterCarousel;
    private GameObject groundModel;
    private GameObject numberModel;
    private int chapter;
    private bool isUnlocked;
    private bool isSelected;

    public int Chapter => chapter;
    
    public void Init(UI_ChapterCarousel chapterCarousel, int chapter, bool isUnlocked, int acquiredStarCount, int stageCount)
    {
        this.chapterCarousel = chapterCarousel;
        this.chapter = chapter;
        this.isUnlocked = isUnlocked;

        var chapterData = GameManager.Instance.Chapter.GetData(chapter);
        groundModel = Instantiate(chapterData.TileModel, transform);
        numberModel = Instantiate(isUnlocked ? chapterData.NumberModel : lockedNumber, transform);
        numberModel.transform.localScale = new Vector3(numberScale, numberScale, numberScale);
        numberModelDefaultLocalPosition = numberModel.transform.localPosition;
        starCountText.text = string.Format(StarCountFormat, acquiredStarCount, stageCount);
        starCountText.transform.SetParent(numberModel.transform);

        if (isUnlocked && acquiredStarCount == stageCount)
        {
            ApplyCompletedGlow();
        }

        if (isUnlocked)
        {
            return;
        }
        starCountText.gameObject.SetActive(false);
        ApplyLockedColor(groundModel);
        ApplyLockedColor(numberModel);
    }

    private void Update()
    {
        if (isSelected)
        {
            var rotateSpeed = rightClick.action.IsPressed() ? fastGroundRotateSpeed : groundRotateSpeed;
            groundModel.transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.Self);
        }

        var localPosition = numberModelDefaultLocalPosition;
        localPosition.y += Mathf.Sin(Time.time * numberFloatSpeed) * numberFloatHeight;
        numberModel.transform.localPosition = localPosition;
    }

    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
    }

    private void ApplyLockedColor(GameObject model)
    {
        foreach (var renderer in model.GetComponentsInChildren<Renderer>())
        {
            foreach (var material in renderer.materials)
            {
                material.mainTexture = null;
                material.color = lockedColor;
            }
        }
    }

    private void ApplyCompletedGlow()
    {
        numberModel.AddComponent<UI_CompletedChapterGlow>().Init(completedGlowMaterial, completedGlowFlowSpeed);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        chapterCarousel.ClickChapter(chapter, isUnlocked);
    }
}
