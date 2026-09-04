using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance;
    private static Managers Instance
    {
        get
        {
            if (_instance == null)
            {
                if (IsQuit)
                {
                    return null;
                }

                Init();
            }

            return _instance;
        }
    }
    
    private static bool _isQuitting;
    public static bool IsQuit => _isQuitting || !Application.isPlaying;


    #region NO_MONOBEHAVIOUR

    private InputManager _input = new();
    public static InputManager Input => Instance?._input;
    
    private UIManager _ui = new();
    public static UIManager UI => Instance?._ui;
    
    private ResourceManager _resource = new();
    public static ResourceManager Resource => Instance?._resource;

    private PoolManager _pool = new();
    public static PoolManager Pool => Instance?._pool;
    
    private EventManager _event = new();
    public static EventManager Event => Instance?._event;
    
    #endregion


    #region MONOBEHAVIOUR

    private static ScenesManager _scene;
    public static ScenesManager Scene
    {
        get
        {
            if (_scene == null)
            {
                if (IsQuit)
                {
                    return null;
                }

                GameObject go = new GameObject(nameof(ScenesManager));
                go.transform.SetParent(Instance.transform);
                _scene = go.GetOrAddComponent<ScenesManager>();
            }

            return _scene;
        }
    }

    #endregion


    private void Update()
    {
        if (IsQuit)
        {
            return;
        }
        
        UI.OnUpdate();
        Input.OnUpdate();
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    private static void Init()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (_instance != null)
        {
            return;
        }

        GameObject go = new GameObject("@Managers");
        _instance = go.AddComponent<Managers>();
        DontDestroyOnLoad(go);
    }
}