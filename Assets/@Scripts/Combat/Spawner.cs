using System;
using UnityEngine;

/// <summary> 자식 스폰 포인트를 기준으로 적을 생성하고 모든 적의 처치 여부를 관리한다. </summary>
public class Spawner : MonoBehaviour
{
    #region ===== 상태 =====

    private int _spawnedEnemyCount;
    private int _deadEnemyCount;
    private bool _hasRaisedAllEnemiesDead;

    #endregion ===== 상태 =====

    public event Action OnAllEnemiesDead;

    #region ===== 스폰 =====

    /// <summary> 활성화된 모든 자식 스폰 포인트에 적을 생성한다. </summary>
    public void SpawnEnemies()
    {
        ResetSpawnState();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform spawnPoint = transform.GetChild(i);

            if (!spawnPoint.gameObject.activeSelf)
            {
                continue;
            }

            SpawnEnemy(spawnPoint);
        }

        CheckAllEnemiesDead();
    }

    /// <summary> 스폰 진행 상태를 초기화한다. </summary>
    private void ResetSpawnState()
    {
        _spawnedEnemyCount = 0;
        _deadEnemyCount = 0;
        _hasRaisedAllEnemiesDead = false;
    }

    /// <summary> 지정된 스폰 포인트에 적을 생성하고 사망 이벤트를 구독한다. </summary>
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

    #endregion ===== 스폰 =====

    #region ===== 이벤트 =====

    /// <summary> 적이 사망하면 이벤트 구독을 해제하고 처치 여부를 갱신한다. </summary>
    private void HandleEnemyDead(EnemyCtrl enemy)
    {
        if (enemy != null)
        {
            enemy.OnDead -= HandleEnemyDead;
        }

        _deadEnemyCount++;

        CheckAllEnemiesDead();
    }

    /// <summary> 모든 적이 처지되었다는 이벤트를 발생한다. </summary>
    private void RaiseAllEnemiesDead()
    {
        OnAllEnemiesDead?.Invoke();
    }
    
    /// <summary> 생성된 모든 적이 처치되었는지 확인하고 완료 이벤트를 발생시킨다. </summary>
    private void CheckAllEnemiesDead()
    {
        if (_hasRaisedAllEnemiesDead)
        {
            return;
        }

        if (_spawnedEnemyCount == 0 || _deadEnemyCount < _spawnedEnemyCount)
        {
            return;
        }

        RaiseAllEnemiesDead();
        
        _hasRaisedAllEnemiesDead = true;
    }

    #endregion ===== 이벤트 =====
}