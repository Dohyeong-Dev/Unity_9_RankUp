using UnityEngine;

public class ChaseState : BaseEnemyState
{
    public ChaseState(EnemyStateMachine stateMachine, EnemyCtrl enemy) : base(stateMachine, enemy)
    {
    }

    public override void Enter()
    {
    }

    public override void Update(float deltaTime)
    {
        if (Enemy.Target == null)
        {
            StateMachine.ChangeState<IdleState>();
        }
        else
        {
            CPrint.Log(Vector3.Distance(Enemy.transform.position, Enemy.Target.position) + "거리");
        }
    }

    public override void Exit()
    {
    }
}