using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseUI : MonoBehaviour
{
    // Type : Button, Image, Text ...
    protected readonly Dictionary<Type, UnityEngine.Object[]> UIDictionary = new();

    protected void Bind<T>(Type enumType) where T : UnityEngine.Object
    {
        string[] enumListNames = Enum.GetNames(enumType);
        UnityEngine.Object[] objects = new UnityEngine.Object[enumListNames.Length];

        UIDictionary.Add(typeof(T), objects);

        for (int i = 0; i < enumListNames.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
            {
                Transform tr = gameObject.FindChild<Transform>(enumListNames[i], true);

                if (tr != null)
                {
                    objects[i] = tr.gameObject;
                }
            }
            else
            {
                objects[i] = gameObject.FindChild<T>(enumListNames[i], true);
            }

            if (objects[i] == null)
            {
                CPrint.Error($"{typeof(T).Name} 타입의 [{enumListNames[i]}]을(를) 찾지 못했습니다.");
            }
        }
    }

    public T Get<T>(Enum enumValue) where T : UnityEngine.Object
    {
        if (!UIDictionary.TryGetValue(typeof(T), out UnityEngine.Object[] objects))
        {
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