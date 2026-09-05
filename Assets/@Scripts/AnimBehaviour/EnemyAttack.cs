using UnityEngine;

public class EnemyAttack : StateMachineBehaviour
{
    private EnemyCtrl _enemy;

    // 현재 State에 진입한 후 경과 시간
    private float _stateElapsedTime;

    // 현재 State에서 공격 실행 여부
    private bool _isAttackExecuted;

    [Header("공격 실행")]
    [Tooltip("State 진입 후 실제 공격을 실행하기까지 대기하는 시간")]
    [SerializeField] private float _attackDelay = 0.2f;


    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.TryGetComponent(out _enemy))
        {
            CPrint.Error("EnemyCtrl를 찾을 수 없습니다.");
            return;
        }

        _stateElapsedTime = 0f;
        _isAttackExecuted = false;
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

    /// <summary> 적의 실제 공격을 실행하고 현재 State의 공격 실행 상태를 갱신한다. </summary>
    private void ExecuteAttack()
    {
        _enemy.ExecuteAttack();
        _isAttackExecuted = true;
    }
}