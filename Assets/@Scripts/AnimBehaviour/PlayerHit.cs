using UnityEngine;

public class PlayerHit : StateMachineBehaviour
{
    private PlayerCtrl _player;
    
    public override void OnStateExit(Animator animator,  AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null && !animator.TryGetComponent(out _player))
        {
            return;
        }

        _player.OnHitAnimationFinished();
    }
}