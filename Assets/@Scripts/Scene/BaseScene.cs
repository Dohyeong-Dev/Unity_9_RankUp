using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    public ScenesManager.SceneType Type { get; private set; }

    protected abstract void OnAwake();

    private void Awake()
    {
        InitializeEventSystem();
        InitializeSceneType();

        Managers.Scene.SetCurrentScene(this);

        OnAwake();
    }

    protected abstract void OnStart();

    private void Start()
    {
        OnStart();
    }

    protected abstract void OnUpdate();

    private void Update()
    {
        OnUpdate();
    }

    public virtual void Clear()
    {
        Managers.UI.Clear();
        Managers.Resource.Clear();
    }

    private void InitializeEventSystem()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();

        if (eventSystem != null)
        {
            return;
        }

        GameObject eventSystemObject = Managers.Resource.Spawn(ResourceKey.Path.Etc + ResourceKey.Name.Event);
        if (eventSystemObject == null)
        {
            CPrint.Error("EventSystem을 생성하지 못했습니다.");
            return;
        }

        eventSystemObject.name = nameof(EventSystem);
    }

    private void InitializeSceneType()
    {
        if (Utils.TryParseEnum(GetType().Name, out ScenesManager.SceneType sceneType))
        {
            Type = sceneType;
            return;
        }

        CPrint.Error($"SceneType을 찾을 수 없습니다. [{GetType().Name}]");
    }
}