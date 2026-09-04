using UnityEngine;

public class EnemyHit : StateMachineBehaviour
{
    private EnemyCtrl _enemy;
    
    public override void OnStateExit(Animator animator,  AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_enemy == null && !animator.TryGetComponent(out _enemy))
        {
            return;
        }

        _enemy.OnHitAnimationFinished();
    }
}