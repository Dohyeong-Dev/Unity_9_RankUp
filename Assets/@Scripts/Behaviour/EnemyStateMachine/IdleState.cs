using UnityEngine;

/// <summary> 현재 타겟의 상태를 확인하고 공격 또는 추적 상태로 전환하는 대기 상태다. </summary>
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

        if (Enemy.CanAttack)
        {
            StateMachine.ChangeState<AttackState>();
            return;
        }

        if (IsWithinChaseStoppingDistance(target))
        {
            return;
        }

        StateMachine.ChangeState<ChaseState>();
    }

    public override void Exit()
    {
    }

    /// <summary> 타겟이 현재 추적 정지 거리 안에 있는지 확인한다. </summary>
    private bool IsWithinChaseStoppingDistance(Transform target)
    {
        float sqrDistance = (target.position - Enemy.transform.position).sqrMagnitude;
        float stoppingDistance = Enemy.ChaseStoppingDistance;
        float sqrStoppingDistance = stoppingDistance * stoppingDistance;

        return sqrDistance <= sqrStoppingDistance;
    }
}