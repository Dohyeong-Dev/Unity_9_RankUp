using UnityEngine;

public class EnemyHit : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EnemyCtrl enemy = animator.GetComponent<EnemyCtrl>();

        if (enemy == null)
        {
            return;
        }

        enemy.OnHitAnimationFinished();
    }
}