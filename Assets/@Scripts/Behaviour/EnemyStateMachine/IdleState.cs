using UnityEngine;

public class IdleState : BaseEnemyState
{
    private const float ChaseDistanceMargin = 0.5f;
    
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

        float sqrDistance = (target.position - Enemy.transform.position).sqrMagnitude;

        float chaseDistance = Enemy.ChaseStoppingDistance + ChaseDistanceMargin;
        float sqrChaseDistance = chaseDistance * chaseDistance;

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