using UnityEngine;

/// <summary> 플레이어 대시 애니메이션 State 진입 시 대시 이동을 시작한다. </summary>
public class PlayerDash : StateMachineBehaviour
{
    #region ===== 참조 =====

    private DashBehaviour _dash;

    #endregion ===== 참조 =====

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!TryResolveDash(animator))
        {
            CPrint.Error("[PlayerDash] DashBehaviour을 찾을 수 없습니다.");
            return;
        }

        _dash.StartDashMovement();
        
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.PlayerDash);
    }

    #region ===== 참조 확인 =====

    /// <summary> DashBehaviour 참조를 반환하고 없으면 Animator에서 찾는다. </summary>
    private bool TryResolveDash(Animator animator)
    {
        if (_dash != null)
        {
            return true;
        }

        return animator.TryGetComponent(out _dash);
    }

    #endregion ===== 참조 확인 =====
}