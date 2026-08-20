using UnityEngine;

public abstract class BaseHUD : BaseUI
{
    private CanvasGroup _canvasGroup;

    public bool IsInputEnabled { get; private set; }

    private void Awake()
    {
        _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

        Managers.UI.SetupCanvas(gameObject, this);

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

    // HUD UI의 표시 여부 설정
    public void SetVisible(bool visible)
    {
        _canvasGroup.alpha = visible ? 1f : 0f;
    }

    // HUD의 입력 처리 활성화 여부 설정
    public void SetInputEnabled(bool enabled)
    {
        IsInputEnabled = enabled;
    }
}