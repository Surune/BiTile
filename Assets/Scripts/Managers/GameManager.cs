using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager s_instance;
    public static GameManager Instance { get { Init(); return s_instance; } }

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
        if (s_instance != null)
        {
            return;
        }
        
        var go = GameObject.Find("@Managers");
        if (go == null)
        {
            go = new GameObject { name = "@Managers" };
            go.AddComponent<GameManager>();
        }
        DontDestroyOnLoad(go);
        s_instance = go.GetComponent<GameManager>();

        s_instance._sound.Init();
    }
}
