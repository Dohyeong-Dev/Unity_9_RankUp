using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary> Unity 프로젝트에서 자주 사용하는 기능을 확장 메서드로 제공한다. </summary>
public static class Extension
{
    /// <summary> GameObject에서 컴포넌트를 가져오고 없으면 추가한다. </summary>
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        return Utils.GetOrAddComponent<T>(go);
    }

    /// <summary> 현재 GameObject의 부모에서 지정한 타입의 오브젝트를 찾는다. </summary>
    public static T FindParent<T>(this GameObject go) where T : Object
    {
        return Utils.FindParent<T>(go);
    }

    /// <summary> 현재 GameObject의 자식에서 지정한 타입의 오브젝트를 찾는다. </summary>
    public static T FindChild<T>(this GameObject go, string name = null, bool recursive = false) where T : Object
    {
        return Utils.FindChild<T>(go, name, recursive);
    }

    /// <summary> Dictionary에서 지정한 Value에 해당하는 Key를 찾는다. </summary>
    public static bool TryFindKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value,
        out TKey key)
    {
        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
        {
            if (!EqualityComparer<TValue>.Default.Equals(pair.Value, value))
            {
                continue;
            }

            key = pair.Key;
            return true;
        }

        key = default;
        return false;
    }

    /// <summary> UI에 이벤트를 구독시킨다. </summary>
    public static void BindEvent(this GameObject go, UIEventType eventType, Action action)
    {
        Utils.BindEvent(go, eventType, action);
    }
    
    /// <summary> UI의 모든 이벤트를 구독해제시킨다. </summary>
    public static void ClearEvent(this GameObject go)
    {
        Utils.ClearEvent(go);
    }
    
    /// <summary> 지정한 시간 후 GameObject를 제거한다. </summary>
    public static void DestroyGO(this GameObject go, float seconds = 0f)
    {
        Utils.DestroyGO(go, seconds);
    }
}