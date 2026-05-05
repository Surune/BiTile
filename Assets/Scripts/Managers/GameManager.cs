using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get { Init(); return instance; } }
    static GameManager instance;

    #region  Core

    public ColorManager Color => _color;
    public SoundManager Sound => _sound;
    public StageSelectionState StageSelection => _stageSelection;
    
    private ColorManager _color = new ColorManager();
    private SoundManager _sound = new SoundManager();
    private StageSelectionState _stageSelection = new StageSelectionState();
    
    #endregion
    
    private void Awake()
    {
        Init();
    }

    private static void Init()
    {
        if (instance != null)
        {
            return;
        }
        
        var go = GameObject.Find("@GameManager");
        if (go != null)
        {
            return;
        }
        
        go = new GameObject("@GameManager");
        DontDestroyOnLoad(go);
            
        instance = go.AddComponent<GameManager>();
            
        instance._color.Init();
        instance._sound.Init();
    }
}
