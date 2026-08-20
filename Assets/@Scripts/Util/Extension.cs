using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Extension
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        return Utils.GetOrAddComponent<T>(go);
    }

    public static T FindParent<T>(this GameObject go) where T : Object
    {
        return Utils.FindParent<T>(go);
    }

    public static T FindChild<T>(this GameObject go, string name = null, bool recursive = false) where T : Object
    {
        return Utils.FindChild<T>(go, name, recursive);
    }

    public static bool TryFindKeyByValue<K, V>(this Dictionary<K, V> dict, V value, out K key)
    {
        foreach (var pair in dict)
        {
            if (EqualityComparer<V>.Default.Equals(pair.Value, value))
            {
                key = pair.Key;
                return true;
            }
        }

        key = default;
        return false;
    }

    public static void Destroy(this GameObject go, float seconds = 0f)
    {
        Utils.DestroyGo(go, seconds);
    }
}