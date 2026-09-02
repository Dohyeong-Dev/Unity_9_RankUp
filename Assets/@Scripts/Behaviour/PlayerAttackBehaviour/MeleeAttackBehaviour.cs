using UnityEngine;

public class MeleeAttackBehaviour : BaseAttackBehaviour
{
    private enum ComboStep
    {
        First = 1,
        Second,
        Third,
    }

    // 현재 콤보 단계
    private ComboStep _currentComboStep;

    // 다음 공격 입력이 저장되었는지 여부
    private bool _isComboInputBuffered;

    // 현재 칼집 모션 중인지 여부
    private bool _isSheathing;

    // 칼집 모션이 이동으로 취소되었는지 여부
    private bool _isSheatheCancelled;


    [Header("공격 판정")]
    [SerializeField] private AttackRange _meleeAttackRange;
    [SerializeField] private LayerMask _meleeLayerMask;


    [Header("이펙트")]
    [SerializeField] private ParticleSystem _slashVFX;


    protected override void OnUpdate()
    {
        if (Managers.Input.MouseDown_Left)
        {
            TryAttack();
        }
    }

    public void Clear()
    {
        Player.UnsetCurAttack();

        _currentComboStep = 0;
        _isComboInputBuffered = false;
        SetSheathing(false);
        SetSheathingCancelled(false);

        Player.Animator.SetInteger(AnimatorKey.Hash.MeleeComboStep, 0);
    }
    
    #region ===== 공격 =====

    /// <summary> 공격을 인풋받았을 때 호출한다. </summary>
    private void TryAttack()
    {
        if (!Player.IsDefaultBehaviour)
        {
            return;
        }

        // 공격 중이 아니라면 Attack1 시작
        if (!Player.IsAttacking)
        {
            if (Player.HasEnoughSp(RequiredSp))
            {
                StartAttack(ComboStep.First);
            }

            return;
        }

        // 이미 입력이 저장되어 있으면 무시
        if (_isComboInputBuffered)
        {
            return;
        }

        // 다음 공격 예약
        _isComboInputBuffered = true;
    }

    /// <summary> 지정한 콤보 단계의 공격을 시작한다. </summary>
    private void StartAttack(ComboStep comboStep)
    {
        Player.SetCurAttack(this);
        Player.SetSp(-RequiredSp);

        _currentComboStep = comboStep;
        Player.Animator.SetInteger(AnimatorKey.Hash.MeleeComboStep, (int)_currentComboStep);

        _isComboInputBuffered = false;
    }

    /// <summary> 입력 버퍼가 존재하면 다음 콤보 공격으로 전환한다. </summary>
    public void TryTransitionCombo()
    {
        if (!_isComboInputBuffered || !Player.IsAttacking)
        {
            return;
        }

        if (!Player.HasEnoughSp(RequiredSp))
        {
            _isComboInputBuffered = false;
            return;
        }

        StartAttack(GetNextComboStep());
    }

    /// <summary> 현재 콤보의 다음 단계를 반환한다. </summary>
    private ComboStep GetNextComboStep()
    {
        _currentComboStep++;

        if ((int)_currentComboStep > Utils.GetEnumCount(typeof(ComboStep)))
        {
            _currentComboStep = ComboStep.First;
        }

        return _currentComboStep;
    }

    /// <summary> 현재 공격의 근접 공격 판정을 수행한다. </summary>
    public void CheckMeleeAttackRange()
    {
        if (_meleeAttackRange == null)
        {
            return;
        }

        Collider[] colliders = _meleeAttackRange.GetColliders(_meleeLayerMask);

        if (colliders.Length == 0)
        {
            return;
        }

        Player.Cam.ShakeCamera();

        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(GetRandomDamage());
            }
        }
    }

    #endregion ===== 공격 =====

    #region ===== 시즈 =====

    /// <summary> 칼집 모션 진행 여부를 설정한다. </summary>
    public void SetSheathing(bool value)
    {
        _isSheathing = value;
    }

    /// <summary> 칼집 모션 진행의 취소를 설정한다. </summary>
    private void SetSheathingCancelled(bool value)
    {
        _isSheatheCancelled = value;
    }

    /// <summary> 칼집 모션 중 이동하면 칼집 모션을 취소한다. </summary>
    public void CancelSheathe()
    {
        if (!_isSheathing || _isSheatheCancelled)
        {
            return;
        }

        SetSheathingCancelled(true);

        Player.Animator.SetTrigger(AnimatorKey.Hash.DoCancelSheathe);
        Clear();
    }

    #endregion ===== 시즈 =====

    #region ===== 이펙트 =====

    public void SetSlashVFXTransform(Vector3 localPosition,  Vector3 localEulerAngles)
    {
        if (_slashVFX == null)
        {
            return;
        }

        Transform tr = _slashVFX.transform;

        tr.localPosition = localPosition;
        tr.localEulerAngles = localEulerAngles;
    }

    public void PlaySlashVFX()
    {
        if (_slashVFX == null)
        {
            CPrint.Warning("슬래시 파티클 없음");
            return;
        }

        _slashVFX.Play();
    }

    #endregion ===== 이펙트 =====
}