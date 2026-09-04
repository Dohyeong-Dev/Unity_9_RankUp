public class HitState : BaseEnemyState
{
    private const float HitRecoveryTime = 1f;
    
    private bool _isAnimationFinished;

    public HitState(EnemyStateMachine stateMachine, EnemyCtrl enemy) : base(stateMachine, enemy)
    {
    }

    public override void Enter()
    {
        _isAnimationFinished = false;

        Enemy.StopMovement();

        Enemy.Animator.SetTrigger(AnimatorKey.Hash.DoHit);
    }

    public override void Update(float deltaTime)
    {
        if (StateMachine.StateElapsedTime < HitRecoveryTime)
        {
            return;
        }
        
        // 애니메이션 종료 신호를 받기 전까지 HitState 유지
        if (!_isAnimationFinished)
        {
            return;
        }
        
        if (Enemy.Target != null)
        {
            StateMachine.ChangeState<ChaseState>();
            return;
        }

        StateMachine.ChangeState<ReturnState>();
    }

    public override void Exit()
    {
    }

    public void SetAnimationFinished()
    {
        _isAnimationFinished = true;
    }
}