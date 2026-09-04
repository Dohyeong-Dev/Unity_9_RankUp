using System;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private int _spawnedEnemyCount;
    private int _defeatedEnemyCount;

    public event Action OnAllEnemiesDefeated;

    
    /// <summary> 모든 자식 위치에 적을 생성합니다. </summary>
    public void SpawnEnemies()
    {
        _spawnedEnemyCount = 0;
        _defeatedEnemyCount = 0;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform spawnPoint = transform.GetChild(i);

            if (!spawnPoint.gameObject.activeSelf)
            {
                continue;
            }

            SpawnEnemy(spawnPoint);
        }

        CheckAllEnemiesDefeated();
    }

    
    /// <summary> 지정된 위치에 적을 생성합니다. </summary>
    private void SpawnEnemy(Transform spawnPoint)
    {
        PoolObj enemyObj = Managers.Pool.Get(PoolKey.Path.EnemyMelee);

        if (enemyObj == null)
        {
            CPrint.Warning("Enemy no found!");
            return;
        }

        if (!enemyObj.TryGetComponent(out EnemyCtrl enemy))
        {
            return;
        }

        _spawnedEnemyCount++;

        enemy.OnDead += HandleEnemyDead;

        enemy.Spawn(spawnPoint);
    }

    
    private void HandleEnemyDead(EnemyCtrl enemy)
    {
        enemy.OnDead -= HandleEnemyDead;

        _defeatedEnemyCount++;

        CheckAllEnemiesDefeated();
    }

    
    private void CheckAllEnemiesDefeated()
    {
        if (_spawnedEnemyCount == 0)
        {
            return;
        }

        if (_defeatedEnemyCount < _spawnedEnemyCount)
        {
            return;
        }

        OnAllEnemiesDefeated?.Invoke();
    }
}