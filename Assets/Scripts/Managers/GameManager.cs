using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-10000)]
public class GameManager : MonoBehaviour
{
    private const string LastSelectedModeKey = "LastSelectedMode";
    private const string LastSelectedStageKey = "LastSelectedStage";

    public static GameManager Instance => instance;
    private static GameManager instance;

    public ChapterManager Chapter => chapter;
    public SoundManager Sound => _sound;
    public StageSelectionState StageSelection => _stageSelection;
    public Localization Localization => _localization;
    
    [SerializeField] private InputActionReference toggleFullscreen;
    [SerializeField] private ChapterDataList normalChapterDataList;
    [SerializeField] private ChapterDataList hardChapterDataList;
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

        _stageSelection = LoadStageSelection();
        
        chapter = new ChapterManager(normalChapterDataList, hardChapterDataList);
        _sound.Init(soundDictionary);
        _localization.Init();
        DisplayModeManager.Init();
    }

    private void Update()
    {
        if (toggleFullscreen.action.WasPressedThisFrame())
        {
            DisplayModeManager.ToggleFullScreen();
        }
    }

    public void SetChapter(int chapter)
    {
        if (_stageSelection.Chapter != chapter)
        {
            _stageSelection.Stage = 1;
        }

        _stageSelection.Chapter = chapter;
        SaveStageSelection();
    }

    public void SetStage(int chapter, int stage)
    {
        _stageSelection.Chapter = chapter;
        _stageSelection.Stage = stage;
        SaveStageSelection();
    }

    public void SetMode(Definitions.GameMode mode)
    {
        if (_stageSelection.Mode != mode)
        {
            _stageSelection.Chapter = 1;
            _stageSelection.Stage = 1;
        }

        _stageSelection.Mode = mode;
        SaveStageSelection();
    }

    public void ResetStageSelection()
    {
        _stageSelection.Mode = Definitions.GameMode.Normal;
        _stageSelection.Chapter = 1;
        _stageSelection.Stage = 1;
        SaveStageSelection();
    }

    public ChapterData GetChapterData(int chapterNumber)
    {
        return chapter.GetData(_stageSelection.Mode, chapterNumber);
    }

    private static StageSelectionState LoadStageSelection()
    {
        StageSelectionState stageSelection;
        stageSelection.Mode = (Definitions.GameMode)PlayerPrefs.GetInt(
            LastSelectedModeKey,
            (int)Definitions.GameMode.Normal);
        stageSelection.Chapter = 1;
        stageSelection.Stage = PlayerPrefs.GetInt(LastSelectedStageKey, 1);
        return stageSelection;
    }

    private void SaveStageSelection()
    {
        PlayerPrefs.SetInt(LastSelectedModeKey, (int)_stageSelection.Mode);
        PlayerPrefs.SetInt(LastSelectedStageKey, _stageSelection.Stage);
        PlayerPrefs.Save();
    }
}
