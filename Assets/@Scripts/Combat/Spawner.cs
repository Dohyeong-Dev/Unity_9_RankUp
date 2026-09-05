using System;
using UnityEngine;

/// <summary> 자식 스폰 포인트를 기준으로 적을 생성하고 전멸 여부를 관리합니다. </summary>
public class Spawner : MonoBehaviour
{
    private int _spawnedEnemyCount;
    private int _defeatedEnemyCount;
    private bool _hasRaisedAllEnemiesDefeated;

    public event Action OnAllEnemiesDefeated;

    /// <summary> 활성화된 모든 자식 위치에 적을 생성합니다. </summary>
    public void SpawnEnemies()
    {
        _spawnedEnemyCount = 0;
        _defeatedEnemyCount = 0;
        _hasRaisedAllEnemiesDefeated = false;

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

    /// <summary> 지정한 스폰 포인트에 적을 생성하고 사망 이벤트를 구독합니다. </summary>
    private void SpawnEnemy(Transform spawnPoint)
    {
        PoolObj enemyObject = Managers.Pool.Get(PoolKey.Path.EnemyMelee);
        if (enemyObject == null)
        {
            CPrint.Warning("[Spawner] Enemy pool object를 가져오지 못했습니다.");
            return;
        }

        if (!enemyObject.TryGetComponent(out EnemyCtrl enemy))
        {
            CPrint.Error("[Spawner] Pool object에 EnemyCtrl이 없습니다.");
            Managers.Pool.Return(enemyObject);
            return;
        }

        _spawnedEnemyCount++;

        enemy.OnDead -= HandleEnemyDead;
        enemy.OnDead += HandleEnemyDead;
        enemy.Spawn(spawnPoint);
    }

    /// <summary> 적이 사망하면 구독을 해제하고 전멸 여부를 갱신합니다. </summary>
    private void HandleEnemyDead(EnemyCtrl enemy)
    {
        if (enemy != null)
        {
            enemy.OnDead -= HandleEnemyDead;
        }

        _defeatedEnemyCount++;
        CheckAllEnemiesDefeated();
    }

    /// <summary> 생성된 모든 적이 처치되었을 때 이벤트를 한 번만 발생시킵니다. </summary>
    private void CheckAllEnemiesDefeated()
    {
        if (_hasRaisedAllEnemiesDefeated || _spawnedEnemyCount == 0 || _defeatedEnemyCount < _spawnedEnemyCount)
        {
            return;
        }

        _hasRaisedAllEnemiesDefeated = true;
        OnAllEnemiesDefeated?.Invoke();
    }
}
