using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager s_instance;
    static GameManager Instance { get { Init(); return s_instance; } }

    #region  Core

    ResourceManager _resource = new ResourceManager();
    SoundManager _sound = new SoundManager();
    UIManager _ui = new UIManager();
    StageSelectionManager _stageSelection = new StageSelectionManager();
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static UIManager UI { get { return Instance._ui; } }
    public static StageSelectionManager StageSelection { get { return Instance._stageSelection; } }
    
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
