using UnityEditor;
using UnityEngine;

public static class FindInactiveObjects
{
    [MenuItem("Tools/Misc/Find Inactive Objects", priority = 0)]
    private static void Find()
    {
        GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (GameObject obj in objects)
        {
            if (!obj.activeInHierarchy)
            {
                Debug.Log($"Inactive: {GetPath(obj.transform)}", obj);
            }
        }
    }

    private static string GetPath(Transform target)
    {
        string path = target.name;

        while (target.parent != null)
        {
            target = target.parent;
            path = $"{target.name}/{path}";
        }

        return path;
    }
}