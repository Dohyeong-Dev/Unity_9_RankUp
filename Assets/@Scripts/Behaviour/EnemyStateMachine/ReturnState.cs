using UnityEngine;

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
            Enemy.MoveTo(Enemy.SpawnPosition, Enemy.WalkSpeed);
            Enemy.RotateTowards(Enemy.SpawnPosition, Enemy.RotationSpeed, deltaTime);
            Enemy.UpdateMovementAnimation();

            float sqrDistance = (Enemy.SpawnPosition - Enemy.transform.position).sqrMagnitude;

            if (sqrDistance > PositionThreshold * PositionThreshold)
            {
                return;
            }

            _isReachedSpawnPosition = true;

            Enemy.StopMovement();
        }
        
        Enemy.transform.rotation = Quaternion.RotateTowards(Enemy.transform.rotation, Enemy.SpawnRotation,
            Enemy.RotationSpeed * deltaTime);

        float angleDifference = Quaternion.Angle(Enemy.transform.rotation, Enemy.SpawnRotation);

        if (angleDifference > RotationThreshold)
        {
            return;
        }

        Enemy.transform.rotation = Enemy.SpawnRotation;

        StateMachine.ChangeState<IdleState>();
    }

    public override void Exit()
    {
    }
}