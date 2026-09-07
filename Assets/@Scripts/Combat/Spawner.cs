using System;
using UnityEngine;

/// <summary> 자식 스폰 포인트를 기준으로 적을 생성하고 모든 적의 처치 여부를 관리한다. </summary>
public class Spawner : MonoBehaviour
{
    #region ===== 설정 =====

    [Header("적 생성 비율")]
    [Range(0f, 100f)]
    [SerializeField] private float _meleeSpawnRatio = 70f;

    [Range(0f, 100f)]
    [SerializeField] private float _rangeSpawnRatio = 30f;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private int _spawnedEnemyCount;
    private int _deadEnemyCount;
    private bool _hasRaisedAllEnemiesDead;

    #endregion ===== 상태 =====

    #region ===== 이벤트 =====

    public event Action OnAllEnemiesDead;

    #endregion ===== 이벤트 =====

    #region ===== 스폰 =====

    /// <summary> 스폰 진행 상태를 초기화한다. </summary>
    private void ResetSpawnState()
    {
        _spawnedEnemyCount = 0;
        _deadEnemyCount = 0;
        _hasRaisedAllEnemiesDead = false;
    }

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

    /// <summary> 지정된 스폰 포인트에 적을 생성하고 사망 이벤트를 구독한다. </summary>
    private void SpawnEnemy(Transform spawnPoint)
    {
        string enemyPoolPath = GetRandomEnemyPoolPath();
        PoolObj enemyObject = Managers.Pool.Get(enemyPoolPath);

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

    /// <summary> 설정된 생성 비율에 따라 생성할 적의 Pool 경로를 반환한다. </summary>
    private string GetRandomEnemyPoolPath()
    {
        float totalRatio = _meleeSpawnRatio + _rangeSpawnRatio;

        if (totalRatio <= 0f)
        {
            CPrint.Warning("[Spawner] 적 생성 비율이 모두 0입니다. 근거리 적을 생성합니다.");
            return PoolKey.Path.EnemyMelee;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalRatio);

        return randomValue < _meleeSpawnRatio ? PoolKey.Path.EnemyMelee : PoolKey.Path.EnemyRange;
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

    /// <summary> 모든 적이 처치되었다는 이벤트를 발생시킨다. </summary>
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

        _hasRaisedAllEnemiesDead = true;

        RaiseAllEnemiesDead();
    }

    #endregion ===== 이벤트 =====
}