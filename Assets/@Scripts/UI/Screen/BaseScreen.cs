using UnityEngine;

public abstract class BaseScreen : BaseUI
{
    protected bool IsClosing;

    private void Awake()
    {
        OnAwake();

        Managers.UI.SetupCanvas(gameObject, this);
        Managers.UI.CloseAllPopupUI();
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
        if (Managers.Input.KeyDown_Esc)
        {
            Close();
        }
    }

    public virtual void Close()
    {
        if (IsClosing)
        {
            return;
        }

        IsClosing = true;

        Managers.UI.CloseAll();
    }
}