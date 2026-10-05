using UnityEngine;

/// <summary> 공격 애니메이션 진행률에 따라 공격 판정과 콤보 전환을 처리한다. </summary>
public class PlayerAttack : StateMachineBehaviour
{
    #region ===== 참조 =====

    private PlayerMeleeAttack _meleeAttack;

    #endregion ===== 참조 =====

    #region ===== 공격 판정 =====

    [Header("공격 판정")]
    
    [Tooltip("공격 애니메이션 전체 진행률 기준으로 공격 판정을 발생시킬 시점")]
    [Range(0f, 1f)]
    [SerializeField] private float _attackRangeProgress = 0.5f;

    private bool _isAttackRangeChecked;

    #endregion ===== 공격 판정 =====

    #region ===== 콤보 전환 =====

    [Header("콤보 전환")]
    
    [Tooltip("입력 버퍼를 확인하여 다음 콤보로 전환할 시점")]
    [Range(0f, 1f)]
    [SerializeField] private float _comboTransitionProgress = 0.6f;

    private bool _isComboTransitionChecked;

    #endregion ===== 콤보 전환 =====

    #region ===== 검 궤적 =====

    [Header("검 궤적")]
    
    [Tooltip("검 궤적을 시작할 공격 애니메이션 진행률")]
    [Range(0f, 1f)]
    [SerializeField] private float _slashStartProgress = 0.1f;

    [Tooltip("검 궤적을 시작할 위치")]
    [SerializeField] private Vector3 _slashPosition;

    [Tooltip("검 궤적을 시작할 회전")]
    [SerializeField] private Vector3 _slashRotation;

    private bool _isSlashVFXPlayed;

    #endregion ===== 검 궤적 =====

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ResolveMeleeAttack(animator);
        ResetState();

        PlayAttackSound();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_meleeAttack == null)
        {
            return;
        }

        float progress = stateInfo.normalizedTime;

        UpdateAttackRange(progress);
        UpdateComboTransition(progress);
        UpdateSlashVFX(progress);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StopAttackSound();
    }

    #region ===== 초기화 =====

    /// <summary> 현재 Animator에서 근접 공격 컴포넌트 참조를 초기화한다. </summary>
    private void ResolveMeleeAttack(Animator animator)
    {
        if (_meleeAttack != null)
        {
            return;
        }

        _meleeAttack = animator.GetComponent<PlayerMeleeAttack>();

        if (_meleeAttack == null)
        {
            CPrint.Error("[PlayerAttack] PlayerMeleeAttack을 찾을 수 없습니다.");
        }
    }

    /// <summary> 현재 공격 State의 실행 상태를 초기화한다. </summary>
    private void ResetState()
    {
        _isAttackRangeChecked = false;
        _isComboTransitionChecked = false;
        _isSlashVFXPlayed = false;
    }

    #endregion ===== 초기화 =====

    #region ===== 공격 판정 =====

    /// <summary> 지정된 진행률에 도달하면 공격 범위 판정을 실행한다. </summary>
    private void UpdateAttackRange(float progress)
    {
        if (_isAttackRangeChecked || progress < _attackRangeProgress)
        {
            return;
        }

        _isAttackRangeChecked = true;
        _meleeAttack.CheckMeleeAttackRange();
    }

    #endregion ===== 공격 판정 =====

    #region ===== 콤보 전환 =====

    /// <summary> 지정된 진행률에 도달하면 다음 콤보 전환을 시도한다. </summary>
    private void UpdateComboTransition(float progress)
    {
        if (_isComboTransitionChecked || progress < _comboTransitionProgress)
        {
            return;
        }

        _isComboTransitionChecked = true;
        _meleeAttack.TryTransitionCombo();
    }

    #endregion ===== 콤보 전환 =====

    #region ===== 검 궤적 =====

    /// <summary> 지정된 진행률에 도달하면 검 궤적을 재생한다. </summary>
    private void UpdateSlashVFX(float progress)
    {
        if (_isSlashVFXPlayed || progress < _slashStartProgress)
        {
            return;
        }

        _isSlashVFXPlayed = true;

        _meleeAttack.SetSlashVFXTransform(_slashPosition, _slashRotation);
        _meleeAttack.PlaySlashVFX();
    }

    #endregion ===== 검 궤적 =====
    
    #region ===== 사운드 =====
    
    /// <summary> 현재 콤보에 따라 공격 사운드를 재생한다. </summary>
    private void PlayAttackSound()
    {
        if (_meleeAttack == null)
        {
            CPrint.Error("[PlayerAttack] PlayerMeleeAttack을 찾을 수 없습니다.");
            return;
        }
        
        switch (_meleeAttack.CurrentComboStep)
        {
            case PlayerMeleeAttack.ComboStep.First:
                Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.KatanaSwing01);
                break;
            case PlayerMeleeAttack.ComboStep.Second:
                Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.KatanaSwing02);
                break;
            case PlayerMeleeAttack.ComboStep.Third:
                Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.KatanaSwing03);
                break;
        }
    }
    
    /// <summary> 현재 콤보에 따라 공격 사운드를 정지한다. </summary>
    private void StopAttackSound()
    {
        if (_meleeAttack == null)
        {
            CPrint.Error("[PlayerAttack] PlayerMeleeAttack을 찾을 수 없습니다.");
            return;
        }
        
        switch (_meleeAttack.CurrentComboStep)
        {
            case PlayerMeleeAttack.ComboStep.First:
                Managers.Sound.StopSfx(ResourceKey.Name.SfxType.KatanaSwing01);
                break;
            case PlayerMeleeAttack.ComboStep.Second:
                Managers.Sound.StopSfx(ResourceKey.Name.SfxType.KatanaSwing02);
                break;
            case PlayerMeleeAttack.ComboStep.Third:
                Managers.Sound.StopSfx(ResourceKey.Name.SfxType.KatanaSwing03);
                break;
        }
    }
    
    #endregion ===== 사운드 =====
}