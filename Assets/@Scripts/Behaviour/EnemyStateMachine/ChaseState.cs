using UnityEngine;

public class ChaseState : BaseEnemyState
{
    public ChaseState(EnemyStateMachine stateMachine, EnemyCtrl enemy) : base(stateMachine, enemy)
    {
    }
    
    public override void Enter()
    {
        Enemy.NavMeshAgent.stoppingDistance = Enemy.ChaseStoppingDistance;
    }

    public override void Update(float deltaTime)
    {
        Transform target = Enemy.Target;

        if (target == null)
        {
            StateMachine.ChangeState<ReturnState>();
            return;
        }

        float sqrDistance = (target.position - Enemy.transform.position).sqrMagnitude;

        if (sqrDistance <= Enemy.ChaseStoppingDistance * Enemy.ChaseStoppingDistance)
        {
            Enemy.StopMovement();

            StateMachine.ChangeState<IdleState>();
            return;
        }

        Enemy.MoveTo(target.position, Enemy.RunSpeed);
        Enemy.RotateTowards(target.position, Enemy.RotationSpeed, deltaTime);
        Enemy.UpdateMovementAnimation();
    }

    public override void Exit()
    {
    }
}