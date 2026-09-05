using UnityEngine;

/// <summary> 비공격 상태에서 일정 시간 후 달리기와 대시를 허용한다. </summary>
public class PlayerNoAttacking : StateMachineBehaviour
{
    #region ===== 참조 =====

    private MoveBehaviour _moveBehaviour;
    private DashBehaviour _dashBehaviour;

    #endregion ===== 참조 =====

    #region ===== 설정 =====

    [Header("달리기")]
    [Tooltip("NoAttacking 진입 후 달리기를 허용하기까지 기다리는 시간")]
    [SerializeField] private float _runEnableDelay = 0.5f;

    [Header("대시")]
    [Tooltip("NoAttacking 진입 후 대시를 허용하기까지 기다리는 시간")]
    [SerializeField] private float _dashEnableDelay = 0.5f;

    #endregion ===== 설정 =====

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        InitializeMovementBehaviour(animator);
        InitializeDashBehaviour(animator);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _moveBehaviour?.SetRunStateAllowed(false);
        _dashBehaviour?.SetDashStateAllowed(false);
    }

    #region ===== 초기화 =====

    /// <summary> 이동 행동 참조를 확보하고 달리기를 허용한다. </summary>
    private void InitializeMovementBehaviour(Animator animator)
    {
        if (!TryResolveMoveBehaviour(animator))
        {
            return;
        }

        _moveBehaviour.SetRunStateAllowed(true, _runEnableDelay);
    }

    /// <summary> 대시 행동 참조를 확보하고 대시를 허용한다. </summary>
    private void InitializeDashBehaviour(Animator animator)
    {
        if (!TryResolveDashBehaviour(animator))
        {
            return;
        }

        _dashBehaviour.SetDashStateAllowed(true, _dashEnableDelay);
    }

    #endregion ===== 초기화 =====

    #region ===== 참조 확인 =====

    /// <summary> MoveBehaviour 참조를 반환하고 없으면 Animator에서 찾는다. </summary>
    private bool TryResolveMoveBehaviour(Animator animator)
    {
        if (_moveBehaviour != null)
        {
            return true;
        }

        if (animator.TryGetComponent(out _moveBehaviour))
        {
            return true;
        }

        CPrint.Error("[PlayerNoAttacking] MoveBehaviour을 찾을 수 없습니다.");
        return false;
    }

    /// <summary> DashBehaviour 참조를 반환하고 없으면 Animator에서 찾는다. </summary>
    private bool TryResolveDashBehaviour(Animator animator)
    {
        if (_dashBehaviour != null)
        {
            return true;
        }

        if (animator.TryGetComponent(out _dashBehaviour))
        {
            return true;
        }

        CPrint.Error("[PlayerNoAttacking] DashBehaviour을 찾을 수 없습니다.");
        return false;
    }

    #endregion ===== 참조 확인 =====
}