using UnityEngine;

public class PlayerDie : StateMachineBehaviour
{
    private PlayerCtrl _player;
    private bool _isFinished;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            _player = animator.GetComponent<PlayerCtrl>();
        }
        _isFinished = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_isFinished)
        {
            return;
        }

        // 죽음 애니메이션이 끝났는지 확인
        if (stateInfo.normalizedTime >= 1f)
        {
            _isFinished = true;
            _player.OnDeathAnimationEnd();
        }
    }
}