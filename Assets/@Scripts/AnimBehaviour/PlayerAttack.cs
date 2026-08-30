using UnityEngine;

public class PlayerAttack : StateMachineBehaviour
{
    private MeleeAttackBehaviour _meleeAttack;

    private bool _isAttackRangeChecked;
    private bool _isComboTransitionChecked;

    [Header("공격 판정")]
    [Tooltip("공격 애니메이션 전체 진행률 기준으로 공격 판정을 발생시킬 시점")]
    [Range(0f, 1f)]
    [SerializeField] private float _attackRangeProgress = 0.5f;

    
    [Header("콤보 전환")]
    [Tooltip("입력 버퍼를 확인하여 다음 콤보로 전환할 시점")]
    [Range(0f, 1f)]
    [SerializeField] private float _comboTransitionProgress = 0.6f;

    
    [Header("검 궤적")]
    [Tooltip("검 궤적을 시작할 공격 애니메이션 진행률")]
    [Range(0f, 1f)]
    [SerializeField] private float _slashStartProgress = 0.1f;
    
    [Tooltip("검 궤적을 시작할 위치")]
    [SerializeField] private Vector3 _slashPosition;
    
    [Tooltip("검 궤적을 시작할 회전")]
    [SerializeField] private Vector3 _slashRotation;

    private bool _isSlashVFXPlayed;
    

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_meleeAttack == null)
        {
            _meleeAttack = animator.GetComponent<MeleeAttackBehaviour>();
        }

        _isAttackRangeChecked = false;
        _isComboTransitionChecked = false;
        _isSlashVFXPlayed = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_meleeAttack == null)
        {
            CPrint.Error("MeleeAttack no found!");
            return;
        }

        float progress = stateInfo.normalizedTime;

        // 공격 판정
        if (!_isAttackRangeChecked && progress >= _attackRangeProgress)
        {
            _isAttackRangeChecked = true;
            _meleeAttack.CheckMeleeAttackRange();
        }

        // 콤보 전환
        if (!_isComboTransitionChecked && progress >= _comboTransitionProgress)
        {
            _isComboTransitionChecked = true;
            _meleeAttack.TryTransitionCombo();
        }

        // 검 궤적 시작
        if (!_isSlashVFXPlayed && progress >= _slashStartProgress)
        {
            _isSlashVFXPlayed = true;
            
            _meleeAttack.SetSlashVFXTransform(_slashPosition, _slashRotation);
            _meleeAttack.PlaySlashVFX();
        }
    }
}