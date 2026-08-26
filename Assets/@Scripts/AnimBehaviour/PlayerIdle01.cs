using UnityEngine;

public class PlayerIdle01 : StateMachineBehaviour
{
    private PlayerCtrl _player;

    [Header("Idle02 랜덤 전환")]
    [SerializeField] private float _randMinTime = 8;
    [SerializeField] private float _randMaxTime = 15;
    private float _startTime;
    private float _randTime;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            _player = animator.GetComponent<PlayerCtrl>();
        }
        
        _randTime = Random.Range(_randMinTime, _randMaxTime);
        _startTime = Time.time;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            return;
        }
        
        if (_player.IsMoving || _player.IsAttacking)
        {
            _startTime = Time.time;
        }
        
        if (Time.time - _startTime > _randTime && !animator.IsInTransition(0) && !_player.IsMoving && _player.IsDefaultBehaviour)
        {
            animator.SetTrigger(AnimatorKey.Hash.DoIdleChange);
        }
    }
}