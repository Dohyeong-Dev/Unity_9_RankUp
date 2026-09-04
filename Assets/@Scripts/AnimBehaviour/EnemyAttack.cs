using UnityEngine;

public class EnemyAttack : StateMachineBehaviour
{
    private EnemyCtrl _enemy;
    
    // 현재 State 진입 후 경과 시간
    private float _elapsedTime;

    // 공격이 이미 실행되었는지 여부
    private bool _hasAttacked;

    [Header("공격 실행")]
    [Tooltip("State 진입 후 실제 공격이 실행되기까지의 시간")]
    [SerializeField] private float _attackTimeDelay = 0.2f;


    public override void OnStateEnter(Animator animator,  AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.TryGetComponent(out _enemy))
        {
            CPrint.Error("EnemyCtrl no found!");
        }
        
        _elapsedTime = 0f;
        _hasAttacked = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_enemy)
        {
            return;
        }
        
        if (_hasAttacked)
        {
            return;
        }

        _elapsedTime += Time.deltaTime;

        if (_elapsedTime < _attackTimeDelay)
        {
            return;
        }

        _enemy.ExecuteAttack();
        
        _hasAttacked = true;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_enemy)
        {
            return;
        }
        
        _enemy.RaiseAttackFinished();
    }
}