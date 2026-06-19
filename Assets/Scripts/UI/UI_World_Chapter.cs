using UnityEngine;
using UnityEngine.EventSystems;

public class UI_World_Chapter : MonoBehaviour
{
    [SerializeField] private float groundRotateSpeed = 5f;
    [SerializeField] private float numberFloatSpeed = 1f;
    [SerializeField] private float numberFloatHeight = 0.1f;

    private Vector3 numberModelDefaultLocalPosition;
    private UI_ChapterCarousel chapterCarousel;
    private GameObject groundModel;
    private GameObject numberModel;
    private int chapter;
    private bool isUnlocked;

    public int Chapter => chapter;
    
    public void Init(UI_ChapterCarousel chapterCarousel, int chapter, bool isUnlocked)
    {
        this.chapterCarousel = chapterCarousel;
        this.chapter = chapter;
        this.isUnlocked = isUnlocked;

        var chapterData = GameManager.Instance.Chapter.GetData(chapter);
        groundModel = Instantiate(chapterData.TileModel, transform);
        numberModel = Instantiate(chapterData.NumberModel, transform);
        numberModelDefaultLocalPosition = numberModel.transform.localPosition;
    }

    private void Update()
    {
        groundModel.transform.Rotate(Vector3.up * groundRotateSpeed * Time.deltaTime, Space.Self);

        var localPosition = numberModelDefaultLocalPosition;
        localPosition.y += Mathf.Sin(Time.time * numberFloatSpeed) * numberFloatHeight;
        numberModel.transform.localPosition = localPosition;
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        chapterCarousel.ClickChapter(chapter, isUnlocked);
    }
}
