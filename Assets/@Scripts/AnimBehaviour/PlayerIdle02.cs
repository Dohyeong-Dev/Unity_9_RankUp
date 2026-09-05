using UnityEngine;

/// <summary> 플레이어 행동 상태가 변경되면 Idle01 상태로 복귀한다. </summary>
public class PlayerIdle02 : StateMachineBehaviour
{
    #region ===== 참조 =====

    private PlayerCtrl _player;

    #endregion ===== 참조 =====

    #region ===== 상태 =====

    private bool _isReturningToIdle01;

    #endregion ===== 상태 =====

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!TryResolvePlayer(animator))
        {
            return;
        }

        _isReturningToIdle01 = false;
        animator.ResetTrigger(AnimatorKey.Hash.DoIdleChange);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null || _isReturningToIdle01)
        {
            return;
        }

        if (IsIdle02StateValid())
        {
            return;
        }

        ReturnToIdle01(animator);
    }

    #region ===== Idle 전환 =====

    /// <summary> Idle02 상태를 계속 유지할 수 있는지 반환한다. </summary>
    private bool IsIdle02StateValid()
    {
        return _player.IsDefaultBehaviour
               && !_player.IsMoving
               && !_player.IsAttacking
               && !_player.IsDashing;
    }

    /// <summary> Idle01 상태로 복귀하는 Trigger를 실행한다. </summary>
    private void ReturnToIdle01(Animator animator)
    {
        _isReturningToIdle01 = true;

        animator.ResetTrigger(AnimatorKey.Hash.DoIdleChange);
        animator.SetTrigger(AnimatorKey.Hash.DoIdleChange);
    }

    #endregion ===== Idle 전환 =====

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

        CPrint.Error("[PlayerIdle02] PlayerCtrl을 찾을 수 없습니다.");
        return false;
    }

    #endregion ===== 참조 확인 =====
}