public abstract class BaseEnemyState
{
    protected EnemyStateMachine StateMachine;
    protected EnemyCtrl Enemy;
    
    public BaseEnemyState(EnemyStateMachine stateMachine, EnemyCtrl enemy)
    {
        StateMachine = stateMachine;
        Enemy = enemy;
    }
    
    public abstract void Enter();

    public abstract void Update(float deltaTime);

    public abstract void Exit();
}