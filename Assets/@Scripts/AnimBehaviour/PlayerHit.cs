using UnityEngine;

/// <summary> 플레이어 피격 애니메이션이 종료되면 피격 완료 처리를 전달한다. </summary>
public class PlayerHit : StateMachineBehaviour
{
    #region ===== 참조 =====

    private PlayerCtrl _player;

    #endregion ===== 참조 =====

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.PlayerHit);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!TryResolvePlayer(animator))
        {
            return;
        }

        _player.OnHitAnimationFinished();
    }

    #region ===== 참조 확인 =====

    /// <summary> PlayerCtrl 참조를 반환하고 없으면 Animator에서 찾는다. </summary>
    private bool TryResolvePlayer(Animator animator)
    {
        if (_player != null)
        {
            return true;
        }

        return animator.TryGetComponent(out _player);
    }

    #endregion ===== 참조 확인 =====
}