using UnityEngine;

/// <summary> 화면에 지속적으로 표시되는 HUD UI의 기본 기능을 제공하는 추상 클래스다. </summary>
public abstract class BaseHUD : BaseUI
{
    public override int SortingOrder => 0;
    
    #region ===== 참조 =====

    private CanvasGroup _canvasGroup;

    #endregion ===== 참조 =====

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

    #region ===== 설정 =====

    /// <summary> HUD UI가 Raycast를 받을 수 있는지 설정한다. </summary>
    public void SetRaycastEnabled(bool enabled)
    {
        _canvasGroup.blocksRaycasts = enabled;
    }

    /// <summary> HUD UI의 상호작용 가능 여부를 설정한다. </summary>
    public void SetInteractEnabled(bool enabled)
    {
        _canvasGroup.interactable = enabled;
    }

    /// <summary> HUD UI의 표시 여부를 설정한다. </summary>
    public void SetVisible(bool visible)
    {
        _canvasGroup.alpha = visible ? 1f : 0f;
    }

    #endregion ===== 설정 =====
}