using UnityEngine;

/// <summary> 게임 전역에서 사용하는 매니저들을 생성하고 관리한다. </summary>
public class Managers : MonoBehaviour
{
    private static bool _isQuitting;
    public static bool IsQuit => _isQuitting || !Application.isPlaying;
    
    #region ===== 인스턴스 =====

    private static Managers _instance;

    private static Managers Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            if (IsQuit || !TryInitialize())
            {
                return null;
            }

            return _instance;
        }
    }

    #endregion ===== 인스턴스 =====

    #region ===== 일반 매니저 =====

    private readonly InputManager _input = new();
    public static InputManager Input => Instance?._input;

    private readonly UIManager _ui = new();
    public static UIManager UI => Instance?._ui;

    private readonly ResourceManager _resource = new();
    public static ResourceManager Resource => Instance?._resource;

    private readonly PoolManager _pool = new();
    public static PoolManager Pool => Instance?._pool;

    private readonly EventManager _event = new();
    public static EventManager Event => Instance?._event;

    #endregion ===== 일반 매니저 =====

    #region ===== MonoBehaviour 매니저 =====

    private static ScenesManager _scene;

    public static ScenesManager Scene
    {
        get
        {
            if (_scene != null)
            {
                return _scene;
            }

            if (IsQuit || Instance == null)
            {
                return null;
            }

            InitializeSceneManager();
            return _scene;
        }
    }

    private static SoundManager _sound;

    public static SoundManager Sound
    {
        get
        {
            if (_sound != null)
            {
                return _sound;
            }

            if (IsQuit || Instance == null)
            {
                return null;
            }

            InitializeSoundManager();
            return _sound;
        }
    }
    
    #endregion ===== MonoBehaviour 매니저 =====

    private void Update()
    {
        if (IsQuit)
        {
            return;
        }

        _ui.OnUpdate();
        _input.OnUpdate();
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    #region ===== 초기화 =====

    /// <summary> Managers 싱글톤 인스턴스를 생성하고 초기화한다. </summary>
    private static bool TryInitialize()
    {
        if (!Application.isPlaying)
        {
            return false;
        }

        if (_instance != null)
        {
            return false;
        }
        
        GameObject managerObject = new GameObject("@Managers");

        _instance = managerObject.AddComponent<Managers>();

        DontDestroyOnLoad(managerObject);

        return true;
    }

    /// <summary> SceneManager 오브젝트를 생성하고 Managers 하위에 등록한다. </summary>
    private static void InitializeSceneManager()
    {
        if (Instance == null || _scene != null)
        {
            return;
        }

        GameObject sceneManagerObject = new GameObject(nameof(ScenesManager));
        sceneManagerObject.transform.SetParent(Instance.transform);

        _scene = sceneManagerObject.GetOrAddComponent<ScenesManager>();
    }

    /// <summary> SoundManager 오브젝트를 생성하고 Managers 하위에 등록한다. </summary>
    private static void InitializeSoundManager()
    {
        if (Instance == null || _sound != null)
        {
            return;
        }

        GameObject soundManagerObject = new GameObject(nameof(SoundManager));
        soundManagerObject.transform.SetParent(Instance.transform);

        _sound = soundManagerObject.GetOrAddComponent<SoundManager>();
    }
    
    #endregion ===== 초기화 =====
}