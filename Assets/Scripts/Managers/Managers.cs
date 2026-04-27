using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }

    #region  Core

    ResourceManager _resource = new ResourceManager();
    SoundManager _sound = new SoundManager();
    UIManager _ui = new UIManager();
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static UIManager UI { get { return Instance._ui; } }
    public static PuzzleManager Puzzle
    {
        get { return PuzzleManager.Instance; }
    }
    #endregion
    
    void Start()
    {
        Init();
    }

    static void Init()
    {
        if (s_instance != null)
        {
            return;
        }
        
        var go = GameObject.Find("@Managers");
        if (go == null)
        {
            go = new GameObject { name = "@Managers" };
            go.AddComponent<Managers>();
        }
        DontDestroyOnLoad(go);
        s_instance = go.GetComponent<Managers>();

        s_instance._sound.Init();
    }
}
