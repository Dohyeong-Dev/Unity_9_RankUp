using UnityEngine;

public class PlayerIdle02 : StateMachineBehaviour
{
    private PlayerCtrl _player;
    private bool _isReturningToIdle01;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            _player = animator.GetComponent<PlayerCtrl>();
        }

        _isReturningToIdle01 = false;

        // 이전 Trigger가 남아있지 않도록 초기화
        animator.ResetTrigger(AnimatorKey.Hash.DoIdleChange);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            return;
        }

        if (_isReturningToIdle01)
        {
            return;
        }

        if (!_player.IsDefaultBehaviour || _player.IsMoving || _player.IsAttacking)
        {
            _isReturningToIdle01 = true;

            animator.ResetTrigger(AnimatorKey.Hash.DoIdleChange);
            animator.SetTrigger(AnimatorKey.Hash.DoIdleChange);
        }
    }
}