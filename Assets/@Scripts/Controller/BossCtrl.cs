public class BossCtrl : EnemyCtrl
{
    public override void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        RaiseDead();

        KillTweens();

        StopMovement();
        ClearCombatTarget();

        _fieldOfView?.ClearTarget();

        _collider.enabled = false;

        ResetRigidbodyForDeath();

        _navMeshAgent.enabled = false;

        SetAnimationMoveSpeed(0f);

        if (_animator != null)
        {
            _animator.CrossFade(AnimatorKey.Hash.Die, 0.02f);
        }
    }
}
