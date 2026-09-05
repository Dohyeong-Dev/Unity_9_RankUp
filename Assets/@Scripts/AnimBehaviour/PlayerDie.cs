using UnityEngine;

/// <summary> 플레이어 사망 애니메이션 종료 시 사망 완료 처리를 전달한다. </summary>
public class PlayerDie : StateMachineBehaviour
{
    #region ===== 참조 =====

    private PlayerCtrl _player;

    #endregion ===== 참조 =====

    #region ===== 상태 =====

    private bool _isDeathAnimationFinished;

    #endregion ===== 상태 =====

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!TryResolvePlayer(animator))
        {
            return;
        }

        _isDeathAnimationFinished = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null || _isDeathAnimationFinished)
        {
            return;
        }

        if (stateInfo.normalizedTime < 1f)
        {
            return;
        }

        _isDeathAnimationFinished = true;
        _player.OnDeadAnimationEnded();
    }

    #region ===== 참조 확인 =====

    /// <summary> PlayerCtrl 참조를 반환하고 없으면 Animator에서 찾는다. </summary>
    private bool TryResolvePlayer(Animator animator)
    {
        if (_player != null)
        {
            return true;
        }

        if (animator.TryGetComponent(out _player))
        {
            return true;
        }

        CPrint.Error("[PlayerDie] PlayerCtrl을 찾을 수 없습니다.");
        return false;
    }

    #endregion ===== 참조 확인 =====
}