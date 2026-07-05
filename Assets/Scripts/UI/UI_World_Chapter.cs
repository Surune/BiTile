using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UI_World_Chapter : MonoBehaviour, IPointerClickHandler
{
    [Header("Input")]
    [SerializeField] private InputActionReference rightClick;
    
    [Header("Visual")]
    [SerializeField] private float groundRotateSpeed = 5f;
    [SerializeField] private float fastGroundRotateSpeed = 180f;
    [SerializeField] private float numberScale = 0.9f;
    [SerializeField] private float numberFloatSpeed = 1f;
    [SerializeField] private float numberFloatHeight = 0.1f;
    [SerializeField] private GameObject lockedNumber;
    [SerializeField] private Color lockedColor;

    private Vector3 numberModelDefaultLocalPosition;
    private UI_ChapterCarousel chapterCarousel;
    private GameObject groundModel;
    private GameObject numberModel;
    private int chapter;
    private bool isUnlocked;
    private bool isSelected;

    public int Chapter => chapter;
    
    public void Init(UI_ChapterCarousel chapterCarousel, int chapter, bool isUnlocked)
    {
        this.chapterCarousel = chapterCarousel;
        this.chapter = chapter;
        this.isUnlocked = isUnlocked;

        var chapterData = GameManager.Instance.Chapter.GetData(chapter);
        groundModel = Instantiate(chapterData.TileModel, transform);
        numberModel = Instantiate(isUnlocked ? chapterData.NumberModel : lockedNumber, transform);
        numberModel.transform.localScale = new Vector3(numberScale, numberScale, numberScale);
        numberModelDefaultLocalPosition = numberModel.transform.localPosition;

        if (isUnlocked)
        {
            return;
        }
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

    public void OnPointerClick(PointerEventData eventData)
    {
        chapterCarousel.ClickChapter(chapter, isUnlocked);
    }
}
