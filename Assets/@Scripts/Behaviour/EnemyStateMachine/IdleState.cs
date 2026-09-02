using UnityEngine;

public class IdleState : BaseEnemyState
{
    public IdleState(EnemyStateMachine stateMachine, EnemyCtrl enemy) : base(stateMachine, enemy)
    {
    }

    public override void Enter()
    {
        Enemy.Animator.SetBool(AnimatorKey.Hash.IsMove, false);
        Enemy.Animator.SetFloat(AnimatorKey.Hash.Speed, 0f);
        
        RotateTowardsPlayer();
    }

    public override void Update(float deltaTime)
    {
        CheckTarget();
    }

    public override void Exit()
    {
    }

    private void RotateTowardsPlayer()
    {
        if (Enemy.Player == null)
        {
            return;
        }
        
        Vector3 direction = Enemy.Player.transform.position - Enemy.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > Mathf.Epsilon)
        {
            Enemy.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void CheckTarget()
    {
        if (Enemy.Target == null)
        {
            return;
        }

        if (Enemy.CanAttack)
        {
            StateMachine.ChangeState<AttackState>();
        }
        else
        {
            StateMachine.ChangeState<ChaseState>();
        }
    }
}