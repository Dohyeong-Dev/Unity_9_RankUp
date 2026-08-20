using System;
using UnityEngine;

public class Utils
{
    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
        {
            component = go.AddComponent<T>();
        }

        return component;
    }

    public static T FindParent<T>(GameObject go) where T : UnityEngine.Object
    {
        if (go == null)
        {
            return null;
        }

        while (go != null)
        {
            T component = go.GetComponent<T>();

            if (component != null)
            {
                return component;
            }

            Transform parent = go.transform.parent;

            if (parent == null)
            {
                break;
            }

            go = parent.gameObject;
        }

        CPrint.Error($"{typeof(T).Name}에 해당하는 부모가 존재하지 않습니다.");
        return null;
    }

    public static T FindChild<T>(GameObject go, string name, bool recursive) where T : UnityEngine.Object
    {
        if (go == null)
        {
            return null;
        }

        if (!recursive)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform tr = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || tr.name.Equals(name))
                {
                    T component = tr.GetComponentInChildren<T>(true);
                    if (component != null)
                    {
                        return component;
                    }
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>(true))
            {
                if (string.IsNullOrEmpty(name) || component.name.Equals(name))
                {
                    return component;
                }
            }
        }

        CPrint.Error($"{name}에 해당 컴포넌트를 발견하지 못했습니다.");
        return null;
    }

    public static void DestroyGo(GameObject go, float delay)
    {
        UnityEngine.Object.Destroy(go, delay);
    }

    public static bool TryParseEnum<TEnum>(string enumName, out TEnum value) where TEnum : struct, Enum
    {
        return Enum.TryParse(enumName, ignoreCase: true, out value);
    }

    public static int GetEnumCount(Type enumType)
    {
        return Enum.GetValues(enumType).Length;
    }

    public static Color GetHexColor(string hexCode)
    {
        if (ColorUtility.TryParseHtmlString(hexCode, out Color color))
        {
            return color;
        }
        
        CPrint.Warning($"올바르지 않은 헥스코드 [{hexCode}]");
        return Color.white;
    }

    public static void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}