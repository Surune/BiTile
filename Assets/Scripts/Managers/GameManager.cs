using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance => instance;
    private static GameManager instance;

    #region  Core

    public ColorManager Color => _color;
    public SoundManager Sound => _sound;
    public StageSelectionState StageSelection => _stageSelection;
    
    [SerializeField] private ColorPreset colorPreset;
    [SerializeField] private SoundDictionary soundDictionary;

    private ColorManager _color = new ColorManager();
    private SoundManager _sound = new SoundManager();
    private StageSelectionState _stageSelection = new StageSelectionState();

    #endregion

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        
        _color.Init(colorPreset);
        _sound.Init(soundDictionary);
    }
}
