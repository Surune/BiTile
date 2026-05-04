using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager s_instance;
    public static GameManager Instance { get { Init(); return s_instance; } }

    #region  Core

    public ResourceManager Resource => _resource;
    public SoundManager Sound => _sound;
    public UIManager UI => _ui;
    public StageSelectionState StageSelection => _stageSelection;
    
    private ResourceManager _resource = new ResourceManager();
    private SoundManager _sound = new SoundManager();
    private UIManager _ui = new UIManager();
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
