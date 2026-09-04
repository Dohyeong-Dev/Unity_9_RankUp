using UnityEngine;

public class AttackState : BaseEnemyState
{
    public AttackState(EnemyStateMachine stateMachine, EnemyCtrl enemy) : base(stateMachine, enemy)
    {
    }

    public override void Enter()
    {
        Enemy.OnAttackFinished += HandleAttackFinished;

        if (!Enemy.IsAttackableDistance ||
            Enemy.CurrentAttackBehaviour == null || !Enemy.CurrentAttackBehaviour.IsAvailable)
        {
            StateMachine.ChangeState<ChaseState>();
            return;
        }

        Enemy.StopMovement();

        Enemy.Animator.SetTrigger(AnimatorKey.Hash.DoAttack);
        Enemy.Animator.SetInteger(AnimatorKey.Hash.AttackIndex, Enemy.CurrentAttackBehaviour.AttackAnimationIndex);
    }

    public override void Update(float deltaTime)
    {
        Transform target = Enemy.Target;

        if (target == null)
        {
            return;
        }

        Enemy.RotateTowards(target.position, Enemy.RotationSpeed, deltaTime);
    }

    public override void Exit()
    {
        Enemy.OnAttackFinished -= HandleAttackFinished;
    }

    private void HandleAttackFinished()
    {
        Enemy.Animator.SetInteger(AnimatorKey.Hash.AttackIndex, 0);

        if (Enemy.Target == null)
        {
            StateMachine.ChangeState<ReturnState>();
            return;
        }

        StateMachine.ChangeState<IdleState>();
    }
}