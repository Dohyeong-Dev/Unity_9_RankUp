using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> UI 요소를 Enum 이름을 기준으로 바인딩하고 관리하는 기본 UI 클래스다. </summary>
public abstract class BaseUI : MonoBehaviour
{
    public virtual int SortingOrder => 0;

    // Type : Button, Image, Text ...
    protected readonly Dictionary<Type, UnityEngine.Object[]> UIMap = new();
    
    /// <summary> 지정된 Enum 이름을 기준으로 UI 요소를 찾아 바인딩한다. </summary>
    protected void Bind<T>(Type enumType) where T : UnityEngine.Object
    {
        string[] enumNames = Enum.GetNames(enumType);
        UnityEngine.Object[] objects = new UnityEngine.Object[enumNames.Length];

        if (!UIMap.TryAdd(typeof(T), objects))
        {
            CPrint.Error($"{typeof(T).Name} 타입은 이미 바인딩되어 있습니다.");
            return;
        }

        for (int i = 0; i < enumNames.Length; i++)
        {
            objects[i] = FindUIObject<T>(enumNames[i]);

            if (objects[i] == null)
            {
                CPrint.Error($"{typeof(T).Name} 타입의 [{enumNames[i]}]을(를) 찾지 못했습니다.");
            }
        }
    }

    /// <summary> 이름에 해당하는 UI 요소를 찾아 반환한다. </summary>
    private T FindUIObject<T>(string objectName) where T : UnityEngine.Object
    {
        if (typeof(T) == typeof(GameObject))
        {
            Transform transform = gameObject.FindChild<Transform>(objectName, true);
            return transform != null ? transform.gameObject as T : null;
        }

        return gameObject.FindChild<T>(objectName, true);
    }

    /// <summary> Enum 값에 해당하는 바인딩된 UI 요소를 반환한다. </summary>
    public T Get<T>(Enum enumValue) where T : UnityEngine.Object
    {
        if (!UIMap.TryGetValue(typeof(T), out UnityEngine.Object[] objects))
        {
            CPrint.Error($"{typeof(T).Name} 타입은 바인딩되어 있지 않습니다.");
            return null;
        }

        int index = Convert.ToInt32(enumValue);

        if (index < 0 || index >= objects.Length)
        {
            CPrint.Error($"{typeof(T).Name}의 인덱스 [{index}]가 범위를 벗어났습니다.");
            return null;
        }

        return objects[index] as T;
    }
}