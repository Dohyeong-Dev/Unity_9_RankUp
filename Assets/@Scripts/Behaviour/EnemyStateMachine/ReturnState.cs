using UnityEngine;

/// <summary> 전투를 종료한 적이 초기 스폰 위치와 회전으로 복귀하는 상태다. </summary>
public class ReturnState : BaseEnemyState
{
    private const float PositionThreshold = 0.1f;
    private const float RotationThreshold = 1f;

    private bool _isReachedSpawnPosition;

    public ReturnState(EnemyStateMachine stateMachine, EnemyCtrl enemy) : base(stateMachine, enemy)
    {
    }

    public override void Enter()
    {
        _isReachedSpawnPosition = false;

        Enemy.NavMeshAgent.stoppingDistance = PositionThreshold;
        Enemy.MoveTo(Enemy.SpawnPosition, Enemy.WalkSpeed);
    }

    public override void Update(float deltaTime)
    {
        if (Enemy.Target != null)
        {
            StateMachine.ChangeState<ChaseState>();
            return;
        }

        if (!_isReachedSpawnPosition)
        {
            MoveToSpawnPosition(deltaTime);
            return;
        }

        RotateToSpawnRotation(deltaTime);
    }

    public override void Exit()
    {
    }

    /// <summary> 적을 초기 스폰 위치까지 이동시킨다. </summary>
    private void MoveToSpawnPosition(float deltaTime)
    {
        Enemy.MoveTo(Enemy.SpawnPosition, Enemy.WalkSpeed);
        Enemy.RotateTowards(Enemy.SpawnPosition, Enemy.RotationSpeed, deltaTime);
        Enemy.UpdateMovementAnimation();

        if (!IsReachedSpawnPosition())
        {
            return;
        }

        _isReachedSpawnPosition = true;

        Enemy.StopMovement();
    }

    /// <summary> 현재 위치가 초기 스폰 위치에 도달했는지 확인한다. </summary>
    private bool IsReachedSpawnPosition()
    {
        float sqrDistance = (Enemy.SpawnPosition - Enemy.transform.position).sqrMagnitude;
        float sqrPositionThreshold = PositionThreshold * PositionThreshold;

        return sqrDistance <= sqrPositionThreshold;
    }

    /// <summary> 적을 초기 스폰 회전까지 회전시키고 완료되면 대기 상태로 전환한다. </summary>
    private void RotateToSpawnRotation(float deltaTime)
    {
        Enemy.transform.rotation = Quaternion.RotateTowards(Enemy.transform.rotation, Enemy.SpawnRotation,
            Enemy.RotationSpeed * deltaTime);

        if (!IsReachedSpawnRotation())
        {
            return;
        }

        Enemy.transform.rotation = Enemy.SpawnRotation;

        StateMachine.ChangeState<IdleState>();
    }

    /// <summary> 현재 회전이 초기 스폰 회전에 도달했는지 확인한다. </summary>
    private bool IsReachedSpawnRotation()
    {
        float angleDifference = Quaternion.Angle(Enemy.transform.rotation, Enemy.SpawnRotation);

        return angleDifference <= RotationThreshold;
    }
}