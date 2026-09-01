using System.Collections.Generic;
using UnityEngine;

public class Pool
{
    private GameObject _rootObject;
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

    // Pool에서 생성할 원본 프리팹
    private GameObject _prefab;

    // 현재 사용 가능한 PoolObj
    private readonly Stack<PoolObj> _availableObjects = new();


    /// <summary> Pool 초기화 </summary>
    public void Initialize(GameObject prefab, int initialSize)
    {
        _prefab = prefab;

        for (int i = 0; i < initialSize; i++)
        {
            Return(CreateInstance());
        }
    }

    /// <summary> 새로운 PoolObj 생성 </summary>
    private PoolObj CreateInstance()
    {
        GameObject instance = Object.Instantiate(_prefab, RootObject.transform);

        // (Clone)이 붙지 않도록 원본 프리팹 이름 유지
        instance.name = _prefab.name;

        return instance.GetOrAddComponent<PoolObj>();
    }

    /// <summary> PoolObj를 Pool에 반환 </summary>
    public void Return(PoolObj poolObject)
    {
        if (poolObject == null)
        {
            return;
        }

        poolObject.transform.SetParent(RootObject.transform);
        poolObject.gameObject.SetActive(false);

        _availableObjects.Push(poolObject);
    }

    /// <summary> Pool에서 사용 가능한 PoolObj를 가져온다. 없으면 새로 생성한다. </summary>
    public PoolObj Get()
    {
        PoolObj poolObject;

        if (_availableObjects.Count > 0)
        {
            poolObject = _availableObjects.Pop();
        }
        else
        {
            poolObject = CreateInstance();
        }

        poolObject.gameObject.SetActive(true);

        return poolObject;
    }
}