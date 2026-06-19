using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance => instance;
    private static GameManager instance;

    public ChapterManager Chapter => chapter;
    public SoundManager Sound => _sound;
    public StageSelectionState StageSelection => _stageSelection;
    public Localization Localization => _localization;
    
    [SerializeField] private ChapterDataList chapterDataList;
    [SerializeField] private SoundDictionary soundDictionary;

    private ChapterManager chapter;
    private SoundManager _sound = new SoundManager();
    private StageSelectionState _stageSelection;
    private Localization _localization = new Localization();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        StageSelectionState stageSelection;
        stageSelection.Chapter = 1;
        stageSelection.Stage = 1;
        _stageSelection = stageSelection;
        
        chapter = new ChapterManager(chapterDataList);
        _sound.Init(soundDictionary);
        _localization.Init();
    }

    public void SelectChapter(int chapter)
    {
        _stageSelection.Chapter = chapter;
    }

    public void SelectStage(int chapter, int stage)
    {
        _stageSelection.Chapter = chapter;
        _stageSelection.Stage = stage;
    }
}
