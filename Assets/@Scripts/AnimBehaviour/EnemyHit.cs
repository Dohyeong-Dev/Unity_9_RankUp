using UnityEngine;

/// <summary> 적 피격 애니메이션이 종료되면 피격 완료 처리를 전달한다. </summary>
public class EnemyHit : StateMachineBehaviour
{
    #region ===== 참조 =====

    private EnemyCtrl _enemy;

    #endregion ===== 참조 =====

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!TryResolveEnemy(animator))
        {
            return;
        }

        _enemy.OnHitAnimationFinished();
    }

    #region ===== 참조 확인 =====

    /// <summary> EnemyCtrl 참조를 반환하고 없으면 Animator에서 찾는다. </summary>
    private bool TryResolveEnemy(Animator animator)
    {
        if (_enemy != null)
        {
            return true;
        }

        return animator.TryGetComponent(out _enemy);
    }

    #endregion ===== 참조 확인 =====
}