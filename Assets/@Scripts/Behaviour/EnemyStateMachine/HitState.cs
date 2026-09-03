public class HitState : BaseEnemyState
{
    public HitState(EnemyStateMachine stateMachine, EnemyCtrl enemy) : base(stateMachine, enemy)
    {
    }

    public override void Enter()
    {
        Enemy.StopMovement();

        Enemy.Animator.SetTrigger(AnimatorKey.Hash.DoHit);
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Exit()
    {
    }
}