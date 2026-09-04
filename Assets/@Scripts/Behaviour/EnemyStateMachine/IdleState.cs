using UnityEngine;

public class IdleState : BaseEnemyState
{
    public IdleState(EnemyStateMachine stateMachine, EnemyCtrl enemy) : base(stateMachine, enemy)
    {
    }

    public override void Enter()
    {
        Enemy.StopMovement();
    }

    public override void Update(float deltaTime)
    {
        Transform target = Enemy.Target;

        if (target == null)
        {
            return;
        }

        if (Enemy.IsAttackableDistance &&
            Enemy.CurrentAttackBehaviour != null && Enemy.CurrentAttackBehaviour.IsAvailable)
        {
            StateMachine.ChangeState<AttackState>();
            return;
        }

        float sqrDistance = (target.position - Enemy.transform.position).sqrMagnitude;
        float sqrChaseDistance = Enemy.ChaseStoppingDistance * Enemy.ChaseStoppingDistance;

        if (sqrDistance <= sqrChaseDistance)
        {
            return;
        }

        StateMachine.ChangeState<ChaseState>();
    }

    public override void Exit()
    {
    }
}