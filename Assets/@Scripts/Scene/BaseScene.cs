using UnityEngine;
using UnityEngine.EventSystems;

/// <summary> 모든 게임 씬의 공통 초기화와 정리 기능을 제공하는 기본 씬 클래스다. </summary>
public abstract class BaseScene : MonoBehaviour
{
    public SceneType Type { get; private set; }

    private void Awake()
    {
        InitializeEventSystem();
        InitializeSceneType();

        Managers.Scene.SetCurrentScene(this);
        
        Managers.UI.CloseLoadingUI();

        OnAwake();
    }

    /// <summary> 씬의 공통 초기화가 완료된 후 Awake 단계에서 실행한다. </summary>
    protected abstract void OnAwake();

    private void Start()
    {
        OnStart();
    }

    /// <summary> 씬의 Start 단계에서 실행한다. </summary>
    protected abstract void OnStart();

    private void Update()
    {
        OnUpdate();
    }

    /// <summary> 씬의 Update 단계에서 실행한다. </summary>
    protected abstract void OnUpdate();

    #region ===== 초기화 =====

    /// <summary> EventSystem이 없으면 생성한다. </summary>
    private void InitializeEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = Managers.Resource.Spawn(ResourceKey.Path.Misc + ResourceKey.Name.Event);

        if (eventSystemObject == null)
        {
            CPrint.Error("EventSystem을 생성하지 못했습니다.");
            return;
        }

        eventSystemObject.name = nameof(EventSystem);
    }

    /// <summary> 현재 씬 클래스 이름을 기준으로 SceneType을 초기화한다. </summary>
    private void InitializeSceneType()
    {
        if (Utils.TryParseEnum(GetType().Name, out SceneType sceneType))
        {
            Type = sceneType;
            return;
        }

        CPrint.Error($"SceneType을 찾을 수 없습니다. [{GetType().Name}]");
    }

    /// <summary> 씬에서 사용한 UI, 리소스, 오브젝트 풀을 정리한다. </summary>
    public virtual void Clear()
    {
        Managers.UI.Clear();
        Managers.Resource.Clear();
        Managers.Pool.Clear();
    }

    #endregion ===== 초기화 =====
}