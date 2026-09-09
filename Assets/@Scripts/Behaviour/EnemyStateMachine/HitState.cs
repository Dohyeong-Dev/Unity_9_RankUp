/// <summary> 적이 피해를 입은 후 피격 반응과 회복 시간을 처리하는 상태다. </summary>
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
        if (!CanExitHitState())
        {
            return;
        }

        ChangeNextState();
    }

    public override void Exit()
    {
    }

    /// <summary> 피격 상태를 종료할 수 있는지 확인한다. </summary>
    private bool CanExitHitState()
    {
        return StateMachine.StateElapsedTime >= HitRecoveryTime && _isAnimationFinished;
    }

    /// <summary> 현재 타겟 상태에 따라 다음 행동 상태로 전환한다. </summary>
    private void ChangeNextState()
    {
        if (Enemy.Target != null)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        StateMachine.ChangeState<ReturnState>();
    }

    /// <summary> 피격 애니메이션이 종료되었음을 기록한다. </summary>
    public void SetAnimationFinished()
    {
        _isAnimationFinished = true;
    }
}