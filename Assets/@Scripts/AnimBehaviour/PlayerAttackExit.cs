using UnityEngine;

/// <summary> 공격 종료 후 칼집 애니메이션 중 이동에 따른 칼집 취소를 처리한다. </summary>
public class PlayerAttackExit : StateMachineBehaviour
{
    #region ===== 참조 =====

    private PlayerCtrl _player;
    private PlayerMeleeAttack _meleeAttack;

    #endregion ===== 참조 =====

    #region ===== 상태 =====

    private float _stateElapsedTime;

    #endregion ===== 상태 =====

    #region ===== 설정 =====

    [Header("칼집 모션 중 행동")]
    
    [Tooltip("칼집 모션 시작 후 이 시간이 지나야 이동 가능")]
    [SerializeField, Min(0f)] private float _movementEnableDelay = 0.5f;

    #endregion ===== 설정 =====

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _stateElapsedTime = 0f;

        if (!TryResolveComponents(animator))
        {
            return;
        }

        _meleeAttack.SetSheathing(true);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null || _meleeAttack == null)
        {
            return;
        }

        _stateElapsedTime += Time.deltaTime;

        if (_stateElapsedTime < _movementEnableDelay)
        {
            return;
        }

        if (_player.IsMoving)
        {
            _meleeAttack.CancelSheathe();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_meleeAttack == null)
        {
            return;
        }

        _meleeAttack.Clear();
    }

    #region ===== 참조 확인 =====

    /// <summary> PlayerCtrl과 PlayerMeleeAttack 참조를 확보한다. </summary>
    private bool TryResolveComponents(Animator animator)
    {
        bool hasPlayer = TryResolvePlayer(animator);
        bool hasMeleeAttack = TryResolveMeleeAttack(animator);

        return hasPlayer && hasMeleeAttack;
    }

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

        CPrint.Error("[PlayerAttackExit] PlayerCtrl을 찾을 수 없습니다.");
        return false;
    }

    /// <summary> PlayerMeleeAttack 참조를 반환하고 없으면 Animator에서 찾는다. </summary>
    private bool TryResolveMeleeAttack(Animator animator)
    {
        if (_meleeAttack != null)
        {
            return true;
        }

        if (animator.TryGetComponent(out _meleeAttack))
        {
            return true;
        }

        CPrint.Error("[PlayerAttackExit] PlayerMeleeAttack을 찾을 수 없습니다.");
        return false;
    }

    #endregion ===== 참조 확인 =====
}