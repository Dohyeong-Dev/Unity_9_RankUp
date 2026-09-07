using UnityEngine;

/// <summary> 공격 범위 안의 대상을 탐색하여 근접 공격을 처리한다. </summary>
public class EnemyMeleeAttack : EnemyAttackBehaviour
{
    #region ===== 공격 판정 =====

    [Header("공격 판정")]
    [SerializeField] private AttackRange _meleeAttackRange;
    [SerializeField] private LayerMask _meleeLayerMask;

    #endregion ===== 공격 판정 =====

    #region ===== 공격 =====

    /// <summary> 근접 공격을 실행하고 공격 범위 내의 대상에게 피해를 적용한다. </summary>
    public override void ExecuteAttack()
    {
        base.ExecuteAttack();
        
        CheckMeleeAttackRange();
    }

    /// <summary> 근접 공격 범위 안의 대상을 탐색하여 피해를 적용한다. </summary>
    public void CheckMeleeAttackRange()
    {
        if (_meleeAttackRange == null)
        {
            CPrint.Warning("[EnemyMeleeAttack] AttackRange를 찾을 수 없습니다.");
            return;
        }

        Collider[] colliders = _meleeAttackRange.GetColliders(_meleeLayerMask);

        foreach (Collider collider in colliders)
        {
            ApplyDamage(collider);
        }
    }

    /// <summary> 지정된 충돌체가 공격 가능한 대상이면 피해와 피격 효과를 적용한다. </summary>
    private void ApplyDamage(Collider collider)
    {
        if (!collider.TryGetComponent(out IDamageable damageable))
        {
            return;
        }

        damageable.TakeDamage(GetRandomDamage());

        if (collider.TryGetComponent(out PlayerCtrl player))
        {
            player.PlayHitEffect(transform);
        }
    }

    #endregion ===== 공격 =====
}