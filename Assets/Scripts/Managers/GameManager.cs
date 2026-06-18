using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance => instance;
    private static GameManager instance;

    public ColorManager Color => _color;
    public SoundManager Sound => _sound;
    public StageSelectionState StageSelection => _stageSelection;
    public Localization Localization => _localization;
    
    [SerializeField] private ColorPreset colorPreset;
    [SerializeField] private SoundDictionary soundDictionary;

    private ColorManager _color = new ColorManager();
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
        
        _color.Init(colorPreset);
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
