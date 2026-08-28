using UnityEngine;

public class PlayerAttackExit : StateMachineBehaviour
{
    private PlayerCtrl _player;
    private MeleeAttackBehaviour _meleeAttack;

    // State 진입 후 실제 경과 시간
    private float _stateTimer;

    [Header("칼집 모션 중 행동")]
    [Tooltip("칼집 모션 시작 후 이 시간이 지나야 추가 행동이 가능"), Min(0f)]
    [SerializeField] private float _actionDelay = 0.1f;

    public override void OnStateEnter(Animator animator,  AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            _player = animator.GetComponent<PlayerCtrl>();
        }
        
        if (_meleeAttack == null)
        {
            _meleeAttack = animator.GetComponent<MeleeAttackBehaviour>();
        }

        if (_meleeAttack == null)
        {
            CPrint.Error("MeleeAttack no found!");

            return;
        }

        _stateTimer = 0f;

        // 칼집 State 시작
        _meleeAttack.SetSheathing(true);
    }

    public override void OnStateUpdate(Animator animator,  AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            CPrint.Error("MeleeAttack no found!");

            return;
        }
        
        if (_meleeAttack == null)
        {
            CPrint.Error("MeleeAttack no found!");

            return;
        }

        _stateTimer += Time.deltaTime;

        // 행동 가능 시간이 아직 지나지 않았다면 대기
        if (_stateTimer < _actionDelay)
        {
            return;
        }

        // 행동 가능 시간이 지난 후 이동하면 칼집 모션 취소
        if (_player.IsMoving)
        {
            _meleeAttack.CancelSheathe();
        }
    }

    public override void OnStateExit(Animator animator,  AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_meleeAttack == null)
        {
            return;
        }

        _meleeAttack.SetWeaponTrail(false);
        _meleeAttack.SetSheathing(false);

        _meleeAttack.Clear();
    }
}