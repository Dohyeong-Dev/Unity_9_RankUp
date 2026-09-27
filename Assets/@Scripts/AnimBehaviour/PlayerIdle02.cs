using UnityEngine;

/// <summary> 플레이어 행동 상태가 변경되면 Idle01 상태로 복귀한다. </summary>
public class PlayerIdle02 : StateMachineBehaviour
{
    #region ===== 참조 =====

    private PlayerCtrl _player;
    
    private BlendShapeController _blendShapeController;

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

        _blendShapeController?.SetBlendShape(BlendShapeKey.Player.Smile.Index, 100);
        if (Managers.UI.CurrentScreen == null && Managers.UI.CurrentPopup == null)
        {
            Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.PlayerIdle2);
        }
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

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _blendShapeController?.ResetBlendShape();
    }

    #region ===== Idle 전환 =====

    /// <summary> Idle02 상태를 계속 유지할 수 있는지 반환한다. </summary>
    private bool IsIdle02StateValid()
    {
        return _player.IsDefaultBehaviour && !_player.IsMoving && !_player.IsAttacking && !_player.IsDashing;
    }

    /// <summary> Idle01 상태로 복귀하는 Trigger를 실행한다. </summary>
    private void ReturnToIdle01(Animator animator)
    {
        _isReturningToIdle01 = true;
        _blendShapeController?.ResetBlendShape();

        animator.ResetTrigger(AnimatorKey.Hash.DoIdleChange);
        animator.SetTrigger(AnimatorKey.Hash.DoIdleChange);
    }

    #endregion ===== Idle 전환 =====

    #region ===== 참조 확인 =====

    /// <summary> 플레이어 관련 참조를 연결하고 없으면 Animator에서 찾는다. </summary>
    private bool TryResolvePlayer(Animator animator)
    {
        if (_player != null)
        {
            return true;
        }

        if (animator.TryGetComponent(out _player))
        {
            _blendShapeController = _player.GetComponent<BlendShapeController>();
            
            return true;
        }

        CPrint.Error("[PlayerIdle02] PlayerCtrl을 찾을 수 없습니다.");
        return false;
    }

    #endregion ===== 참조 확인 =====
}