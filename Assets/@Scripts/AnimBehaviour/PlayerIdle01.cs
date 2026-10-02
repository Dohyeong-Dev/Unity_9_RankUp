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

    private float _stateElapsedTime;
    private float _randomIdleTime;

    #endregion ===== 상태 =====

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!TryResolvePlayer(animator))
        {
            return;
        }

        animator.ResetTrigger(AnimatorKey.Hash.DoIdleChange);

        _stateElapsedTime = 0f;
        _randomIdleTime = Random.Range(_randomMinTime, _randomMaxTime);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            return;
        }

        if (_player.IsMoving || _player.IsAttacking || _player.IsDead || _player.IsHit)
        {
            _stateElapsedTime = 0f;
            return;
        }

        if (!IsIdleTransitionReady(animator))
        {
            return;
        }

        _stateElapsedTime += Time.deltaTime;

        if (_stateElapsedTime < _randomIdleTime)
        {
            return;
        }

        RequestIdle02(animator);
    }

    #region ===== Idle 전환 =====

    /// <summary> 현재 Idle02로 전환할 수 있는 상태인지 반환한다. </summary>
    private bool IsIdleTransitionReady(Animator animator)
    {
        return !animator.IsInTransition(0) && _player.IsDefaultBehaviour;
    }

    /// <summary> Idle02 상태로 전환을 요청하는 Trigger를 실행한다. </summary>
    private void RequestIdle02(Animator animator)
    {
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

        CPrint.Error("[PlayerIdle01] PlayerCtrl을 찾을 수 없습니다.");
        return false;
    }

    #endregion ===== 참조 확인 =====
}