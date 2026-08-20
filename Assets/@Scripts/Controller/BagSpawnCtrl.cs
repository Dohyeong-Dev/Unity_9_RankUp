using System.Collections.Generic;
using UnityEngine;

public class BagSpawnCtrl : MonoBehaviour
{
    [SerializeField] private GameObject _bagPrefab;
    [SerializeField] private List<Transform> _spawnPoints;

    private Bag _currentBag;

    private void Start()
    {
        SpawnBag();
    }

    public void SpawnBag()
    {
        if (_bagPrefab == null)
        {
            CPrint.Error("Bag Prefab이 설정되지 않았습니다.");
            return;
        }

        if (_spawnPoints == null || _spawnPoints.Count == 0)
        {
            CPrint.Error("Bag SpawnPoint가 설정되지 않았습니다.");
            return;
        }

        Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
        GameObject bagObject = Instantiate(_bagPrefab, spawnPoint.position, spawnPoint.rotation);

        _currentBag = bagObject.GetComponent<Bag>();
        if (_currentBag == null)
        {
            CPrint.Error("Bag Prefab에 Bag 컴포넌트가 없습니다.");
            Destroy(bagObject);
            return;
        }

        _currentBag.Initialize(this);
    }

    public void OnBagCollected(Vector3 position)
    {
        SpawnBag();
    }
}