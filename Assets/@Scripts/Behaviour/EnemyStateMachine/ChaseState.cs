using UnityEngine;

/// <summary> 현재 전투 타겟을 추적하고 공격 가능한 거리까지 이동하는 상태다. </summary>
public class ChaseState : BaseEnemyState
{
    private float _chaseSpeed;

    public ChaseState(EnemyStateMachine stateMachine, EnemyCtrl enemy) : base(stateMachine, enemy)
    {
    }

    public override void Enter()
    {
        _chaseSpeed = GetRandomChaseSpeed();
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

        if (IsWithinChaseStoppingDistance(target))
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

    /// <summary> 적의 달리기 속도 범위 안에서 추적 속도를 결정한다. </summary>
    private float GetRandomChaseSpeed()
    {
        float minSpeed = Enemy.RunSpeed - Enemy.RunSpeedVariance;
        float maxSpeed = Enemy.RunSpeed + Enemy.RunSpeedVariance;

        float chaseSpeed = Random.Range(minSpeed, maxSpeed);

        return Mathf.Max(Enemy.WalkSpeed + 0.1f, chaseSpeed);
    }

    /// <summary> 타겟이 현재 공격 행동의 추적 정지 거리 안에 있는지 확인한다. </summary>
    private bool IsWithinChaseStoppingDistance(Transform target)
    {
        float sqrDistance = (target.position - Enemy.transform.position).sqrMagnitude;
        float stoppingDistance = Enemy.ChaseStoppingDistance;
        float sqrStoppingDistance = stoppingDistance * stoppingDistance;

        return sqrDistance <= sqrStoppingDistance;
    }
}