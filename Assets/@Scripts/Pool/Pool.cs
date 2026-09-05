using System.Collections.Generic;
using UnityEngine;

/// <summary> 동일한 프리팹 인스턴스를 재사용하는 오브젝트 풀이다. </summary>
public class Pool
{
    private GameObject _rootObject;
    private GameObject _prefab;
    private string _poolKey;

    private readonly Stack<PoolObj> _availableObjects = new();

    #region ===== 루트 =====

    /// <summary> 현재 풀 오브젝트를 보관하는 루트 오브젝트를 반환한다. </summary>
    public GameObject RootObject
    {
        get
        {
            if (_rootObject == null)
            {
                _rootObject = new GameObject($"Pool_{_prefab.name}");
            }

            return _rootObject;
        }
    }

    #endregion ===== 루트 =====

    #region ===== 초기화 =====

    /// <summary> 풀의 프리팹과 식별자를 설정하고 초기 오브젝트를 생성한다. </summary>
    public void Initialize(GameObject prefab, string poolKey, int initialSize)
    {
        if (prefab == null)
        {
            CPrint.Error("[Pool] Pool prefab이 null입니다.");
            return;
        }

        _prefab = prefab;
        _poolKey = poolKey;

        CreateInitialObjects(initialSize);
    }

    /// <summary> 지정한 개수만큼 초기 풀 오브젝트를 생성한다. </summary>
    private void CreateInitialObjects(int initialSize)
    {
        for (int i = 0; i < initialSize; i++)
        {
            PoolObj poolObject = CreateInstance();
            Return(poolObject);
        }
    }

    #endregion ===== 초기화 =====

    #region ===== 생성 =====

    /// <summary> 새로운 풀 오브젝트를 생성하고 소유 풀 정보를 설정한다. </summary>
    private PoolObj CreateInstance()
    {
        GameObject instance = Object.Instantiate(_prefab, RootObject.transform);
        instance.name = $"{_prefab.name}_{instance.transform.GetSiblingIndex()}";

        PoolObj poolObject = instance.GetOrAddComponent<PoolObj>();
        poolObject.SetPoolKey(_poolKey);

        return poolObject;
    }

    #endregion ===== 생성 =====

    #region ===== 대여 =====

    /// <summary> 사용 가능한 풀 오브젝트를 대여하고 없으면 새로 생성한다. </summary>
    public PoolObj Get()
    {
        PoolObj poolObject = GetAvailableObject();

        if (poolObject == null)
        {
            poolObject = CreateInstance();
        }

        poolObject.gameObject.SetActive(true);

        return poolObject;
    }

    /// <summary> 사용 가능한 풀 오브젝트를 하나 반환한다. </summary>
    private PoolObj GetAvailableObject()
    {
        while (_availableObjects.Count > 0)
        {
            PoolObj poolObject = _availableObjects.Pop();

            if (poolObject != null)
            {
                return poolObject;
            }
        }

        return null;
    }

    #endregion ===== 대여 =====

    #region ===== 반환 =====

    /// <summary> 풀 오브젝트를 비활성화하고 사용 가능 목록에 추가한다. </summary>
    public void Return(PoolObj poolObject)
    {
        if (poolObject == null)
        {
            return;
        }

        poolObject.transform.SetParent(RootObject.transform, false);
        poolObject.gameObject.SetActive(false);

        _availableObjects.Push(poolObject);
    }

    #endregion ===== 반환 =====
}