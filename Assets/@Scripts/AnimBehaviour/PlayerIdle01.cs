using UnityEngine;

public class PlayerIdle01 : StateMachineBehaviour
{
    private PlayerCtrl _player;

    [Header("Idle02 랜덤 전환")]
    [SerializeField] private float _randomMinTime = 8f;
    [SerializeField] private float _randomMaxTime = 15f;

    private float _idleStartTime;
    private float _randomIdleTime;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            _player = animator.GetComponent<PlayerCtrl>();
        }

        // 이전 Idle02 전환 Trigger가 남아있는 것을 방지
        animator.ResetTrigger(AnimatorKey.Hash.DoIdleChange);

        _randomIdleTime = Random.Range(_randomMinTime, _randomMaxTime);
        _idleStartTime = Time.time;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            return;
        }

        // 움직이거나 공격 중이면 Idle02 대기 시간을 다시 시작
        if (_player.IsMoving || _player.IsAttacking)
        {
            _idleStartTime = Time.time;
            return;
        }

        if (!IsIdleTransitionReady(animator))
        {
            return;
        }

        if (Time.time - _idleStartTime >= _randomIdleTime)
        {
            animator.SetTrigger(AnimatorKey.Hash.DoIdleChange);

            _idleStartTime = Time.time;
        }
    }

    private bool IsIdleTransitionReady(Animator animator)
    {
        return !animator.IsInTransition(0) && !_player.IsMoving && !_player.IsAttacking &&
               _player.IsDefaultBehaviour;
    }
}