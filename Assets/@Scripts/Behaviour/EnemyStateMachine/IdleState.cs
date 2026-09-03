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
        if (Enemy.Target == null)
        {
            return;
        }

        StateMachine.ChangeState<ChaseState>();
    }

    public override void Exit()
    {
    }
}