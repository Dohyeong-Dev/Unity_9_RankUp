using System.Collections.Generic;
using UnityEngine;

/// <summary> 프리팹별 오브젝트 풀의 생성, 대여 및 반환을 관리한다. </summary>
public class PoolManager
{
    private readonly Dictionary<string, Pool> _poolMap = new();

    private GameObject _rootObject;

    #region ===== 루트 =====

    /// <summary> 모든 오브젝트 풀을 관리하는 루트 오브젝트를 반환한다. </summary>
    private GameObject RootObject
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

    #endregion ===== 루트 =====

    #region ===== 풀 생성 =====

    /// <summary> 지정한 프리팹 경로의 오브젝트 풀을 생성한다. </summary>
    public void CreatePool(string prefabPath, int initialSize = 0)
    {
        if (string.IsNullOrWhiteSpace(prefabPath))
        {
            CPrint.Warning("[PoolManager] Pool prefab path가 비어 있습니다.");
            return;
        }

        if (_poolMap.ContainsKey(prefabPath))
        {
            return;
        }

        GameObject prefab = Managers.Resource.Load<GameObject>(prefabPath);

        if (prefab == null)
        {
            CPrint.Error($"[PoolManager] Pool 프리팹을 찾을 수 없습니다. Path: {prefabPath}");
            return;
        }

        Pool pool = new Pool();
        pool.Initialize(prefab, prefabPath, initialSize);
        pool.RootObject.transform.SetParent(RootObject.transform, false);

        _poolMap.Add(prefabPath, pool);
    }

    /// <summary> 지정한 프리팹 경로의 풀을 반환하고 없으면 새로 생성한다. </summary>
    private Pool GetOrCreatePool(string prefabPath)
    {
        if (_poolMap.TryGetValue(prefabPath, out Pool pool))
        {
            return pool;
        }

        CreatePool(prefabPath);

        return _poolMap.TryGetValue(prefabPath, out pool) ? pool : null;
    }

    #endregion ===== 풀 생성 =====

    #region ===== 대여 =====

    /// <summary> 지정한 프리팹 경로의 풀 오브젝트를 대여한다. </summary>
    public PoolObj Get(string prefabPath)
    {
        if (string.IsNullOrWhiteSpace(prefabPath))
        {
            CPrint.Warning("[PoolManager] Pool prefab path가 비어 있습니다.");
            return null;
        }

        Pool pool = GetOrCreatePool(prefabPath);

        return pool != null ? pool.Get() : null;
    }

    #endregion ===== 대여 =====

    #region ===== 반환 =====

    /// <summary> 풀 오브젝트가 기록한 소유 풀로 반환한다. </summary>
    public bool Return(PoolObj poolObject)
    {
        if (poolObject == null)
        {
            return false;
        }

        string poolKey = poolObject.PoolKey;

        if (string.IsNullOrWhiteSpace(poolKey))
        {
            CPrint.Warning("[PoolManager] PoolKey가 설정되지 않은 오브젝트입니다.");
            return false;
        }

        if (!_poolMap.TryGetValue(poolKey, out Pool pool))
        {
            CPrint.Warning($"[PoolManager] 반환할 Pool을 찾을 수 없습니다. Key: {poolKey}");
            return false;
        }

        pool.Return(poolObject);

        return true;
    }

    #endregion ===== 반환 =====

    #region ===== 정리 =====

    /// <summary> 모든 풀 오브젝트와 관리 정보를 제거한다. </summary>
    public void Clear()
    {
        if (_rootObject != null)
        {
            Object.Destroy(_rootObject);
            _rootObject = null;
        }

        _poolMap.Clear();
    }

    #endregion ===== 정리 =====
}