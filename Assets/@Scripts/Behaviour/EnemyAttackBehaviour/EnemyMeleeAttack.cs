using UnityEngine;

public class EnemyMeleeAttack : EnemyAttackBehaviour
{
    [Header("공격 판정")]
    [SerializeField] private AttackRange _meleeAttackRange;
    [SerializeField] private LayerMask _meleeLayerMask;


    public override void ExecuteAttack()
    {
        base.ExecuteAttack();
        
        CheckMeleeAttackRange();
    }

    public void CheckMeleeAttackRange()
    {
        if (_meleeAttackRange == null)
        {
            return;
        }

        Collider[] colliders = _meleeAttackRange.GetColliders(_meleeLayerMask);

        if (colliders.Length == 0)
        {
            return;
        }

        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(GetRandomDamage());

                if (col.TryGetComponent(out PlayerCtrl player))
                {
                    player.PlayHitEffect(transform);
                }
            }
        }
    }
}