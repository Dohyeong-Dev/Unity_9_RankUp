using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    // 키 : 자료형
    // 값 : 딕셔너리(키 : Path, 값 : 오브젝트)
    private Dictionary<System.Type, Dictionary<string, Object>> _loadedObjDic = new();

    public T Load<T>(string path) where T : Object
    {
        if (!_loadedObjDic.TryGetValue(typeof(T), out var dic))
        {
            dic = new Dictionary<string, Object>();
            _loadedObjDic.Add(typeof(T), dic);
        }

        if (dic.TryGetValue(path, out Object cachedObj))
        {
            T loadedObj = cachedObj as T;

            if (loadedObj != null)
            {
                return loadedObj;
            }

            CPrint.Warning($"{path}<{typeof(T)}> is null");
            dic.Remove(path);
        }

        T loadObj = Resources.Load<T>(path);
        if (loadObj == null)
        {
            CPrint.Error($"Load => Resource[{path}] is null");
            return null;
        }

        dic.Add(path, loadObj);

        return loadObj;
    }

    public GameObject Spawn(GameObject prefab, Transform parent = null)
    {
        if (prefab == null)
        {
            CPrint.Error("Spawn => Prefab is null");
            return null;
        }
        
        GameObject go = GameObject.Instantiate(prefab, parent);
        go.name = prefab.name;
        return go;
    }
    
    public GameObject Spawn(string path, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion),
        Transform parent = null)
    {
        GameObject prefab = Load<GameObject>(path);
        if (prefab == null)
        {
            CPrint.Error($"Spawn -> Resource[{path}] is null");
            return null;
        }
        
        GameObject go = Spawn(prefab, parent);
        go.transform.position = pos;
        go.transform.rotation = rot;

        return go;
    }

    public void Clear()
    {
        _loadedObjDic.Clear();
        // 참조되지 않는 Asset을 정리
        Resources.UnloadUnusedAssets();
    }
}