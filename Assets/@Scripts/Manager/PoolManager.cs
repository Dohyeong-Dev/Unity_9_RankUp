using System.Collections.Generic;
using UnityEngine;

public class PoolManager
{
    private readonly Dictionary<string, Pool> _pools = new();

    private GameObject _rootObject;
    public GameObject RootObject
    {
        get
        {
            if (_rootObject == null)
            {
                _rootObject = new GameObject("Pools");
            }

            return _rootObject;
        }
    }


    /// <summary> 프리팹 경로를 통해 Pool을 생성한다. Pool은 프리팹의 GameObject 이름으로 관리한다. </summary>
    public void CreatePool(string prefabPath, int initialSize = 0)
    {
        GameObject prefab = Managers.Resource.Load<GameObject>(prefabPath);

        if (prefab == null)
        {
            CPrint.Error($"[PoolManager] Pool 프리팹을 찾을 수 없습니다. Path : {prefabPath}");

            return;
        }

        string poolName = prefab.name;

        // 이미 Pool이 존재하면 생성하지 않는다.
        if (_pools.ContainsKey(poolName))
        {
            return;
        }

        Pool pool = new Pool();

        pool.Initialize(prefab, initialSize);
        pool.RootObject.transform.SetParent(RootObject.transform);

        _pools.Add(poolName, pool);
    }

    /// <summary> PoolObj를 해당 Pool에 반환한다. GameObject 이름을 기준으로 Pool을 찾는다. </summary>
    public bool Return(PoolObj poolObject)
    {
        if (poolObject == null)
        {
            return false;
        }

        string poolName = poolObject.gameObject.name;

        int lastUnderscoreIndex = poolName.LastIndexOf('_');

        if (lastUnderscoreIndex >= 0)
        {
            string suffix = poolName.Substring(lastUnderscoreIndex + 1);

            if (int.TryParse(suffix, out _))
            {
                poolName = poolName.Substring(0, lastUnderscoreIndex);
            }
        }

        if (!_pools.TryGetValue(poolName, out Pool pool))
        {
            CPrint.Warning($"반환할 Pool을 찾을 수 없습니다. Name : {poolName}");

            return false;
        }

        pool.Return(poolObject);

        return true;
    }

    /// <summary> 프리팹 경로를 통해 PoolObj를 가져온다. Pool은 내부적으로 프리팹 이름으로 관리된다. </summary>
    public PoolObj Get(string prefabPath)
    {
        GameObject prefab = Managers.Resource.Load<GameObject>(prefabPath);

        if (prefab == null)
        {
            CPrint.Error($"Pool 프리팹을 찾을 수 없습니다. Path : {prefabPath}");

            return null;
        }

        string poolName = prefab.name;

        // Pool이 없으면 새로 생성
        if (!_pools.TryGetValue(poolName, out Pool pool))
        {
            CreatePool(prefabPath);

            // 생성 실패를 대비해 다시 확인
            if (!_pools.TryGetValue(poolName, out pool))
            {
                return null;
            }
        }

        return pool.Get();
    }

    /// <summary> 모든 Pool을 제거한다. </summary>
    public void Clear()
    {
        if (_rootObject != null)
        {
            Object.Destroy(_rootObject);
            _rootObject = null;
        }

        _pools.Clear();
    }
}