public abstract class BaseEnemyState
{
    protected EnemyStateMachine StateMachine;
    protected EnemyCtrl Enemy;

    /// <summary> State에 필요한 참조를 설정합니다. </summary>
    public BaseEnemyState(EnemyStateMachine stateMachine, EnemyCtrl enemy)
    {
        StateMachine = stateMachine;
        Enemy = enemy;
    }

    /// <summary> State 진입 시 호출됩니다. </summary>
    public abstract void Enter();

    /// <summary> State 실행 중 매 프레임 호출됩니다. </summary>
    public abstract void Update(float deltaTime);

    /// <summary> State 종료 시 호출됩니다. </summary>
    public abstract void Exit();
}