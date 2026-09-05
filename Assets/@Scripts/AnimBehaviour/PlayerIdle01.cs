using UnityEngine;

/// <summary> 플레이어가 일정 시간 동안 Idle 상태를 유지하면 Idle02 전환을 요청한다. </summary>
public class PlayerIdle01 : StateMachineBehaviour
{
    #region ===== 참조 =====

    private PlayerCtrl _player;

    #endregion ===== 참조 =====

    #region ===== 설정 =====

    [Header("Idle02 랜덤 전환")]
    [SerializeField] private float _randomMinTime = 8f;
    [SerializeField] private float _randomMaxTime = 15f;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private float _idleWaitingStartTime;
    private float _randomIdleTime;

    #endregion ===== 상태 =====

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!TryResolvePlayer(animator))
        {
            return;
        }

        animator.ResetTrigger(AnimatorKey.Hash.DoIdleChange);

        _randomIdleTime = Random.Range(_randomMinTime, _randomMaxTime);
        _idleWaitingStartTime = Time.time;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            return;
        }

        if (_player.IsMoving || _player.IsAttacking)
        {
            ResetIdleWaitingTime();
            return;
        }

        if (!IsIdleTransitionReady(animator))
        {
            return;
        }

        if (Time.time - _idleWaitingStartTime < _randomIdleTime)
        {
            return;
        }

        animator.SetTrigger(AnimatorKey.Hash.DoIdleChange);
        ResetIdleWaitingTime();
    }

    #region ===== 대기 시간 =====

    /// <summary> Idle02 전환을 위한 대기 시간을 초기화한다. </summary>
    private void ResetIdleWaitingTime()
    {
        _idleWaitingStartTime = Time.time;
    }

    #endregion ===== 대기 시간 =====

    #region ===== 전환 조건 =====

    /// <summary> 현재 Idle02로 전환할 수 있는 상태인지 반환한다. </summary>
    private bool IsIdleTransitionReady(Animator animator)
    {
        return !animator.IsInTransition(0)
               && !_player.IsMoving
               && !_player.IsAttacking
               && _player.IsDefaultBehaviour;
    }

    #endregion ===== 전환 조건 =====

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

        CPrint.Error("[PlayerIdle01] PlayerCtrl을 찾을 수 없습니다.");
        return false;
    }

    #endregion ===== 참조 확인 =====
}