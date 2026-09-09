using UnityEngine;

/// <summary> 적의 공격 타겟을 향해 원거리 발사체를 생성하고 발사한다. </summary>
public class EnemyRangeAttack : EnemyAttackBehaviour
{
    #region ===== 설정 =====

    [Header("발사체")]
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private bool _isGuided = true;
    [SerializeField] private float _lifeTime = 2f;

    [Tooltip("동시에 발사할 발사체 개수")]
    [Min(1)]
    [SerializeField] private int _projectileCount = 1;

    [Tooltip("적의 로컬 좌표를 기준으로 계산되는 발사 위치")]
    [SerializeField] private Vector3 _spawnOffset = new(0f, 1f, 1f);

    [SerializeField] private PoolKey.ProjectileType _projectileType;

    #endregion ===== 설정 =====

    #region ===== 공격 =====

    /// <summary> 현재 타겟을 향해 원거리 발사체를 생성한다. </summary>
    public override void ExecuteAttack()
    {
        if (!IsAvailable)
        {
            return;
        }

        Transform target = Enemy.Target;

        if (target == null)
        {
            return;
        }

        base.ExecuteAttack();
        SpawnProjectiles(target);
    }

    /// <summary> 설정된 개수만큼 적의 전방 180도 범위에 발사체를 생성한다. </summary>
    private void SpawnProjectiles(Transform target)
    {
        Vector3 spawnPosition = Enemy.transform.TransformPoint(_spawnOffset);
        Vector3 centerDirection = GetAttackDirection(target.position, spawnPosition);

        for (int i = 0; i < _projectileCount; i++)
        {
            Vector3 direction = GetSpreadDirection(centerDirection, i);

            SpawnProjectile(target, spawnPosition, direction);
        }
    }

    /// <summary> 설정된 위치와 방향으로 발사체를 생성한다. </summary>
    private void SpawnProjectile(Transform target, Vector3 spawnPosition, Vector3 direction)
    {
        PoolObj poolObj = Managers.Pool.Get(GetProjectilePath());

        if (poolObj == null)
        {
            CPrint.Warning("[EnemyRangeAttack] Projectile pool object를 가져오지 못했습니다.");
            return;
        }

        if (!poolObj.TryGetComponent(out Projectile projectile))
        {
            CPrint.Error("[EnemyRangeAttack] Pool object에 Projectile이 없습니다.");
            Managers.Pool.Return(poolObj);
            return;
        }

        projectile.SetProjectile(Enemy.gameObject, spawnPosition, direction, _moveSpeed, GetRandomDamage(),
            _lifeTime, _isGuided, target);
    }

    /// <summary> 전방 180도 범위를 기준으로 균등하게 분배된 발사 방향을 반환한다. </summary>
    private Vector3 GetSpreadDirection(Vector3 centerDirection, int index)
    {
        float angleStep = 180f / (_projectileCount + 1);
        float angle = angleStep * (index + 1) - 90f;

        return Quaternion.AngleAxis(angle, Vector3.up) * centerDirection;
    }

    /// <summary> 발사 위치에서 타겟을 향하는 수평 방향을 반환한다. </summary>
    private Vector3 GetAttackDirection(Vector3 targetPosition, Vector3 spawnPosition)
    {
        Vector3 direction = targetPosition - spawnPosition;
        direction.y = 0f;

        return direction.normalized;
    }

    /// <summary> 선택된 발사체 타입에 해당하는 풀 경로를 반환한다. </summary>
    private string GetProjectilePath()
    {
        return PoolKey.Path.GetProjectilePath(_projectileType);
    }

    #endregion ===== 공격 =====
}