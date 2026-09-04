using UnityEngine;

public class ChaseState : BaseEnemyState
{
    private float _chaseSpeed;

    public ChaseState(EnemyStateMachine stateMachine, EnemyCtrl enemy) : base(stateMachine, enemy)
    {
    }

    public override void Enter()
    {
        _chaseSpeed = Random.Range(Enemy.RunSpeed - Enemy.RunSpeedVariance, Enemy.RunSpeed + Enemy.RunSpeedVariance);
        _chaseSpeed = Mathf.Max(Enemy.WalkSpeed + 0.1f, _chaseSpeed);
    }

    public override void Update(float deltaTime)
    {
        Transform target = Enemy.Target;

        if (target == null)
        {
            StateMachine.ChangeState<ReturnState>();
            return;
        }

        Enemy.NavMeshAgent.stoppingDistance = Enemy.ChaseStoppingDistance;

        float sqrDistance = (target.position - Enemy.transform.position).sqrMagnitude;
        float sqrStoppingDistance = Enemy.ChaseStoppingDistance * Enemy.ChaseStoppingDistance;

        if (sqrDistance <= sqrStoppingDistance)
        {
            Enemy.StopMovement();
            StateMachine.ChangeState<IdleState>();
            return;
        }

        Enemy.MoveTo(target.position, _chaseSpeed);
        Enemy.RotateTowards(target.position, Enemy.RotationSpeed, deltaTime);
        Enemy.UpdateMovementAnimation();
    }

    public override void Exit()
    {
    }
}