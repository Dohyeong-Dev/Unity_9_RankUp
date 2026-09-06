/// <summary> 적 상태 머신에서 사용하는 모든 상태의 기본 클래스다. </summary>
public abstract class BaseEnemyState
{
    protected readonly EnemyStateMachine StateMachine;
    
    public EnemyCtrl Enemy { get; private set; }

    protected BaseEnemyState(EnemyStateMachine stateMachine, EnemyCtrl enemy)
    {
        StateMachine = stateMachine;
        Enemy = enemy;
    }

    /// <summary> 현재 상태에 진입할 때 호출한다. </summary>
    public abstract void Enter();

    /// <summary> 현재 상태를 갱신한다. </summary>
    public abstract void Update(float deltaTime);

    /// <summary> 현재 상태를 종료할 때 호출한다. </summary>
    public abstract void Exit();
}