using UnityEngine;
using UnityEngine.UI;

/// <summary> 씬 전환을 해도 파괴되지 않는 PersistentRoot에 속하는 오버레이UI의 추상 클래스다. </summary>
[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
public abstract class BaseOverlay : BaseUI
{
    private void Awake()
    {
        Managers.UI.SetupCanvas(this);

        OnAwake();
    }

    protected abstract void OnAwake();
}