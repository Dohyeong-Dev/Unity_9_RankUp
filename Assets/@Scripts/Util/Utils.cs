using System;
using UnityEngine;

/// <summary> 프로젝트 전반에서 사용하는 공통 유틸리티 기능을 제공한다. </summary>
public static class Utils
{
    /// <summary> GameObject에서 지정한 컴포넌트를 가져오고 없으면 추가한다. </summary>
    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();

        if (component != null)
        {
            return component;
        }

        return go.AddComponent<T>();
    }

    /// <summary> 현재 GameObject부터 부모 방향으로 지정한 타입의 컴포넌트를 찾는다. </summary>
    public static T FindParent<T>(GameObject go) where T : UnityEngine.Object
    {
        if (go == null)
        {
            return null;
        }

        Transform currentTransform = go.transform;

        while (currentTransform != null)
        {
            T component = currentTransform.GetComponent<T>();

            if (component != null)
            {
                return component;
            }

            currentTransform = currentTransform.parent;
        }

        CPrint.Error($"{typeof(T).Name}에 해당하는 부모가 존재하지 않습니다.");
        return null;
    }

    /// <summary> 자식 오브젝트에서 지정한 타입의 컴포넌트를 찾는다. </summary>
    public static T FindChild<T>(GameObject go, string name, bool recursive) where T : UnityEngine.Object
    {
        if (go == null)
        {
            return null;
        }

        if (recursive)
        {
            return FindChildRecursive<T>(go, name);
        }

        return FindDirectChild<T>(go, name);
    }

    /// <summary> 직접적인 자식 오브젝트에서 지정한 컴포넌트를 찾는다. </summary>
    private static T FindDirectChild<T>(GameObject go, string name) where T : UnityEngine.Object
    {
        for (int i = 0; i < go.transform.childCount; i++)
        {
            Transform child = go.transform.GetChild(i);

            if (!string.IsNullOrEmpty(name) && child.name != name)
            {
                continue;
            }

            T component = child.GetComponentInChildren<T>(true);

            if (component != null)
            {
                return component;
            }
        }

        CPrint.Error($"{name}에 해당 컴포넌트를 발견하지 못했습니다.");
        return null;
    }

    /// <summary> 모든 하위 자식 오브젝트에서 지정한 컴포넌트를 찾는다. </summary>
    private static T FindChildRecursive<T>(GameObject go, string name) where T : UnityEngine.Object
    {
        foreach (T component in go.GetComponentsInChildren<T>(true))
        {
            if (string.IsNullOrEmpty(name) || component.name == name)
            {
                return component;
            }
        }

        CPrint.Error($"{name}에 해당 컴포넌트를 발견하지 못했습니다.");
        return null;
    }

    /// <summary> 지정한 시간 후 GameObject를 제거한다. </summary>
    public static void DestroyGO(GameObject go, float delay)
    {
        UnityEngine.Object.Destroy(go, delay);
    }

    /// <summary> 문자열을 지정한 Enum 타입으로 변환한다. </summary>
    public static bool TryParseEnum<TEnum>(string enumName, out TEnum value) where TEnum : struct, Enum
    {
        return Enum.TryParse(enumName, ignoreCase: true, out value);
    }

    /// <summary> 지정한 Enum 타입에 정의된 값의 개수를 반환한다. </summary>
    public static int GetEnumCount(Type enumType)
    {
        return Enum.GetValues(enumType).Length;
    }

    /// <summary> UI에 이벤트를 구독시킨다. </summary>
    public static void BindEvent(GameObject go, UIEventType eventType, Action action)
    {
        EventHandler eventHandler = GetOrAddComponent<EventHandler>(go);

        switch (eventType)
        {
            case UIEventType.Click:
                eventHandler.OnClickHandler -= action;
                eventHandler.OnClickHandler += action;
                break;
            case UIEventType.Pressed:
                eventHandler.OnPressedHandler -= action;
                eventHandler.OnPressedHandler += action;
                break;
            case UIEventType.PointerDown:
                eventHandler.OnPointerDownHandler -= action;
                eventHandler.OnPointerDownHandler += action;
                break;
            case UIEventType.PointerUp:
                eventHandler.OnPointerUpHandler -= action;
                eventHandler.OnPointerUpHandler += action;
                break;
            case UIEventType.PointerEnter:
                eventHandler.OnPointerEnterHandler -= action;
                eventHandler.OnPointerEnterHandler += action;
                break;
            case UIEventType.PointerExit:
                eventHandler.OnPointerExitHandler -= action;
                eventHandler.OnPointerExitHandler += action;
                break;
        }
    }

    /// <summary> UI에 등록된 모든 이벤트를 초기화한다. </summary>
    public static void ClearEvent(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        EventHandler eventHandler = go.GetComponent<EventHandler>();

        if (eventHandler == null)
        {
            return;
        }

        eventHandler.ClearAllEvents();
    }

    /// <summary> 실행 중인 애플리케이션을 종료한다. </summary>
    public static void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}