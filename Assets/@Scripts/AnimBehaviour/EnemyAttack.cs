using UnityEngine;

/// <summary> 적 공격 애니메이션 State에서 일정 시간 후 실제 공격을 실행한다. </summary>
public class EnemyAttack : StateMachineBehaviour
{
    #region ===== 참조 =====

    private EnemyCtrl _enemy;

    #endregion ===== 참조 =====

    #region ===== 상태 =====

    private float _stateElapsedTime;
    private bool _isAttackExecuted;

    #endregion ===== 상태 =====

    #region ===== 설정 =====

    [Header("공격 실행")]
    
    [Tooltip("State 진입 후 실제 공격을 실행하기까지 대기하는 시간")]
    [SerializeField] private float _attackDelay = 0.2f;

    #endregion ===== 설정 =====

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        InitializeState(animator);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_enemy == null || _isAttackExecuted)
        {
            return;
        }

        _stateElapsedTime += Time.deltaTime;

        if (_stateElapsedTime < _attackDelay)
        {
            return;
        }

        ExecuteAttack();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_enemy == null)
        {
            return;
        }

        _enemy.RaiseAttackFinished();
    }

    #region ===== 초기화 =====

    /// <summary> 적 참조와 공격 실행 상태를 초기화한다. </summary>
    private void InitializeState(Animator animator)
    {
        if (!animator.TryGetComponent(out _enemy))
        {
            CPrint.Error("[EnemyAttack] EnemyCtrl를 찾을 수 없습니다.");
            return;
        }

        _stateElapsedTime = 0f;
        _isAttackExecuted = false;
    }

    #endregion ===== 초기화 =====

    #region ===== 공격 =====

    /// <summary> 적의 실제 공격을 실행하고 공격 실행 상태를 갱신한다. </summary>
    private void ExecuteAttack()
    {
        _enemy.ExecuteAttack();
        _isAttackExecuted = true;
    }

    #endregion ===== 공격 =====
}