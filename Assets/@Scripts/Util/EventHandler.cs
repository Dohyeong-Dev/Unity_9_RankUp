using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary> UI 오브젝트에서 발생하는 포인터 이벤트를 받아 외부에 전달한다. </summary>
public class EventHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    #region ===== 이벤트 =====

    public event Action OnClickHandler;
    public event Action OnPointerDownHandler;
    public event Action OnPointerUpHandler;
    public event Action OnPointerEnterHandler;
    public event Action OnPointerExitHandler;
    public event Action OnPressedHandler;

    #endregion ===== 이벤트 =====

    private bool _isPressed;

    private void Update()
    {
        if (_isPressed)
        {
            OnPressedHandler?.Invoke();
        }
    }

    /// <summary> 등록된 모든 UI 이벤트를 초기화한다. </summary>
    public void ClearAllEvents()
    {
        OnClickHandler = null;
        OnPressedHandler = null;
        OnPointerDownHandler = null;
        OnPointerUpHandler = null;
        OnPointerEnterHandler = null;
        OnPointerExitHandler = null;
    }

    /// <summary> UI 오브젝트를 클릭했을 때 호출된다. </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickHandler?.Invoke();
    }

    /// <summary> UI 오브젝트를 누르기 시작했을 때 호출된다. </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
        OnPointerDownHandler?.Invoke();
    }

    /// <summary> UI 오브젝트에서 포인터를 뗐을 때 호출된다. </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
        OnPointerUpHandler?.Invoke();
    }

    /// <summary> 포인터가 UI 오브젝트에 들어왔을 때 호출된다. </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterHandler?.Invoke();
    }

    /// <summary> 포인터가 UI 오브젝트에서 나갔을 때 호출된다. </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitHandler?.Invoke();
    }
}