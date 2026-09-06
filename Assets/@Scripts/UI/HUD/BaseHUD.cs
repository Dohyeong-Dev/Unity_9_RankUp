using UnityEngine;

public abstract class BaseHUD : BaseUI
{
    public override int SortingOrder => 0;
    
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

        Managers.UI.SetupCanvas(this);

        OnAwake();
    }

    protected abstract void OnAwake();

    private void Start()
    {
        OnStart();
    }

    protected abstract void OnStart();

    private void Update()
    {
        OnUpdate();
    }

    protected abstract void OnUpdate();

    public virtual void OnInputKey()
    {
    }

    // HUD UI의 Raycast 수신 여부 설정
    public void SetRaycastEnabled(bool enabled)
    {
        _canvasGroup.blocksRaycasts = enabled;
    }

    // HUD UI의 Interact 여부 설정
    public void SetInteractEnabled(bool enabled)
    {
        _canvasGroup.interactable = enabled;
    }
    
    // HUD UI의 표시 여부 설정
    public void SetVisible(bool visible)
    {
        _canvasGroup.alpha = visible ? 1f : 0f;
    }
}