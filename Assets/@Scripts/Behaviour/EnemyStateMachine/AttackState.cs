using UnityEngine;

/// <summary> 공격 가능한 타겟을 향해 공격 애니메이션과 실제 공격을 처리하는 상태다. </summary>
public class AttackState : BaseEnemyState
{
    public AttackState(EnemyStateMachine stateMachine, EnemyCtrl enemy) : base(stateMachine, enemy)
    {
    }

    public override void Enter()
    {
        if (!Enemy.CanAttack)
        {
            StateMachine.ChangeState<ChaseState>();
            return;
        }

        Enemy.OnAttackFinished += HandleAttackFinished;

        Enemy.StopMovement();
        StartAttackAnimation();
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

    /// <summary> 현재 선택된 공격 행동에 맞는 공격 애니메이션을 실행한다. </summary>
    private void StartAttackAnimation()
    {
        Enemy.Animator.SetInteger(AnimatorKey.Hash.AttackIndex, Enemy.CurrentAttackBehaviour.AttackAnimationIndex);
        Enemy.Animator.SetTrigger(AnimatorKey.Hash.DoAttack);
    }

    /// <summary> 공격 애니메이션이 종료된 후 다음 행동을 결정한다. </summary>
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