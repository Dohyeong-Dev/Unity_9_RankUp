using UnityEngine;

public class PlayerIdle02 : StateMachineBehaviour
{
    private PlayerCtrl _player;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            _player = animator.GetComponent<PlayerCtrl>();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            return;
        }
        
        if (!_player.IsDefaultBehaviour || _player.IsMoving || _player.IsAttacking)
        {
            animator.SetTrigger(AnimatorKey.Hash.DoIdleChange);
        }
    }
}