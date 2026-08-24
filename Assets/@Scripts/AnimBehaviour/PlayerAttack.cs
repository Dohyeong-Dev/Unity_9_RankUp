using UnityEngine;

public class PlayerAttack : StateMachineBehaviour
{
    private PlayerCtrl _player;
    private MeleeAttackBehaviour _playerMeleeAttack;

    private bool _isFinished;
    private bool _isRangeChecked;

    [Header("공격 판정")]
    [Range(0f, 1f)]
    [SerializeField] private float _attackRangeCheckProgress = 0.5f;
    [Header("콤보 입력")]
    [Range(0f, 1f)]
    [SerializeField] private float _comboInputStartProgress = 0.6f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            _player = animator.GetComponent<PlayerCtrl>();
        }

        if (_playerMeleeAttack == null)
        {
            _playerMeleeAttack = _player.GetComponent<MeleeAttackBehaviour>();
        }

        _isFinished = false;
        _isRangeChecked = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_player == null)
        {
            CPrint.Error("No PlayerCtrl attached");
            return;
        }

        if (_playerMeleeAttack == null)
        {
            CPrint.Error("No MeleeAttackBehaviour attached");
            return;
        }

        if (_isFinished)
        {
            return;
        }

        float progress = stateInfo.normalizedTime;

        // 공격 판정
        if (!_isRangeChecked && progress >= _attackRangeCheckProgress)
        {
            _isRangeChecked = true;

            _playerMeleeAttack.CheckMeleeAttackRange();
        }

        // 콤보 입력 허용
        if (progress >= _comboInputStartProgress)
        {
            _playerMeleeAttack.SetCanCombo(true);
        }

        // 애니메이션 종료
        if (progress >= 1f)
        {
            _playerMeleeAttack.Clear();
            _isFinished = true;
        }
    }
}