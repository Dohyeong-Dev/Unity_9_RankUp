using UnityEngine;

public class ChaseState : BaseEnemyState
{
    private float _chaseSpeed;

    private const float StuckCheckTime = 1f;
    private const float StuckVelocityThreshold = 0.01f;

    public ChaseState(EnemyStateMachine stateMachine, EnemyCtrl enemy) : base(stateMachine, enemy)
    {
    }

    public override void Enter()
    {
        Enemy.NavMeshAgent.stoppingDistance = Enemy.ChaseStoppingDistance;

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

        float sqrDistance = (target.position - Enemy.transform.position).sqrMagnitude;
        float sqrStoppingDistance = Enemy.ChaseStoppingDistance * Enemy.ChaseStoppingDistance;

        // 실제 목표 거리까지 도착
        if (sqrDistance <= sqrStoppingDistance)
        {
            Enemy.StopMovement();
            StateMachine.ChangeState<IdleState>();
            return;
        }

        Enemy.MoveTo(target.position, _chaseSpeed);
        Enemy.RotateTowards(target.position, Enemy.RotationSpeed, deltaTime);
        Enemy.UpdateMovementAnimation();

        // 일정 시간 동안 이동하지 못하고 있음
        if (StateMachine.StateElapsedTime >= StuckCheckTime &&
            Enemy.NavMeshAgent.velocity.sqrMagnitude <= StuckVelocityThreshold)
        {
            Enemy.StopMovement();
        }
    }

    public override void Exit()
    {
    }
}