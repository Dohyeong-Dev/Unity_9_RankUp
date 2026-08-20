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
                Init();
            }

            return _instance;
        }
    }

    #region NO_MONOBEHAVIOUR

    private InputManager _input = new();
    public static InputManager Input => Instance?._input;
    private UIManager _ui = new();
    public static UIManager UI => Instance?._ui;
    private ResourceManager _resource = new();
    public static ResourceManager Resource => Instance?._resource;

    #endregion

    #region MONOBEHAVIOUR
    
    private static ScenesManager _scene;
    public static ScenesManager Scene
    {
        get
        {
            if (_scene == null)
            {
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
        UI.OnUpdate();
        Input.OnUpdate();
    }

    private static void Init()
    {
        if (_instance == null)
        {
            GameObject go = new GameObject("@Managers");
            _instance = go.AddComponent<Managers>();
            DontDestroyOnLoad(go);
        }
    }
}