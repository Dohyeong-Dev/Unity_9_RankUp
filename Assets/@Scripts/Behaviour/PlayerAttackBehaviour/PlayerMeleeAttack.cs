using UnityEngine;

/// <summary> 플레이어의 근접 공격, 콤보, 공격 방향 및 슬래시 이펙트를 관리한다. </summary>
public class PlayerMeleeAttack : PlayerAttackBehaviour
{
    private enum ComboStep
    {
        First = 1,
        Second,
        Third,
    }

    #region ===== 상태 =====

    private ComboStep _currentComboStep;

    // 다음 콤보 공격 입력이 예약되었는지 여부
    private bool _isComboInputBuffered;

    // 칼집 상태
    private bool _isSheathing;
    private bool _isSheatheCancelled;

    #endregion ===== 상태 =====

    #region ===== 회전 =====

    // 다음 콤보 공격에 사용할 방향
    private Vector3 _nextComboDirection;
    private bool _hasNextComboDirection;

    // FixedUpdate에서 적용할 공격 방향 회전
    private Quaternion _targetAttackRotation;
    private bool _isAttackRotationRequested;

    #endregion ===== 회전 =====

    #region ===== 설정 =====

    [Header("공격 판정")]
    [SerializeField] private AttackRange _meleeAttackRange;
    [SerializeField] private LayerMask _meleeLayerMask;

    [Header("이펙트")]
    [SerializeField] private ParticleSystem _slashVFX;

    #endregion ===== 설정 =====


    protected override void OnFixedUpdate()
    {
        ApplyAttackRotation();
    }

    protected override void OnUpdate()
    {
        UpdateNextComboDirection();

        if (Managers.Input.MouseDown_Left)
        {
            TryAttack();
        }
    }

    #region ===== 초기화 =====

    /// <summary> 현재 공격 상태를 초기화한다. 피격이나 사망 등 공격을 강제로 취소해야 할 때 사용한다. </summary>
    public override void Clear()
    {
        Player.UnsetCurAttack();

        _currentComboStep = 0;
        _isComboInputBuffered = false;

        _nextComboDirection = Vector3.zero;
        _hasNextComboDirection = false;

        _isAttackRotationRequested = false;

        SetSheathing(false);
        SetSheathingCancelled(false);

        Player.Animator.SetInteger(AnimatorKey.Hash.MeleeComboStep, 0);
    }

    #endregion ===== 초기화 =====

    #region ===== 회전 =====

    /// <summary> 현재 입력 방향을 다음 콤보 공격에 사용할 방향으로 저장한다. </summary>
    private void UpdateNextComboDirection()
    {
        Vector3 inputDirection = GetInputDirection();

        if (inputDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        _nextComboDirection = inputDirection;
        _hasNextComboDirection = true;
    }

    /// <summary> 카메라 방향을 기준으로 현재 플레이어의 이동 입력 방향을 계산한다. </summary>
    private Vector3 GetInputDirection()
    {
        if (Player.Cam == null)
        {
            return Vector3.zero;
        }

        Vector2 input = new Vector2(Managers.Input.KeyAxisX, Managers.Input.KeyAxisY);

        if (input.sqrMagnitude <= Mathf.Epsilon)
        {
            return Vector3.zero;
        }

        Transform cameraTransform = Player.Cam.transform;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // 카메라의 위아래 기울기를 제거하여 수평 방향만 사용
        forward.y = 0f;
        right.y = 0f;

        if (forward.sqrMagnitude <= Mathf.Epsilon || right.sqrMagnitude <= Mathf.Epsilon)
        {
            return Vector3.zero;
        }

        forward.Normalize();
        right.Normalize();

        return (forward * input.y + right * input.x).normalized;
    }

    /// <summary> 저장된 다음 콤보 방향을 기준으로 공격 회전을 요청한다. </summary>
    private void RotateTowardsNextComboDirection()
    {
        if (!_hasNextComboDirection || _nextComboDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        _targetAttackRotation = Quaternion.LookRotation(_nextComboDirection);
        _isAttackRotationRequested = true;

        _nextComboDirection = Vector3.zero;
        _hasNextComboDirection = false;
    }

    /// <summary> 요청된 공격 방향 회전을 Rigidbody에 적용한다. </summary>
    private void ApplyAttackRotation()
    {
        if (!_isAttackRotationRequested)
        {
            return;
        }

        Player.Rigid.MoveRotation(_targetAttackRotation);

        _isAttackRotationRequested = false;
    }

    #endregion ===== 회전 =====

    #region ===== 공격 =====

    /// <summary> 현재 상태에 따라 첫 공격을 시작하거나 다음 콤보 공격을 예약한다. </summary>
    private void TryAttack()
    {
        if (!Player.IsDefaultBehaviour)
        {
            return;
        }

        if (Player.IsAttacking)
        {
            BufferNextComboInput();
            return;
        }

        if (!Player.HasEnoughSp(RequiredSp))
        {
            return;
        }

        StartAttack(ComboStep.First);
    }

    /// <summary> 다음 콤보 공격 입력을 예약한다. </summary>
    private void BufferNextComboInput()
    {
        if (_isComboInputBuffered)
        {
            return;
        }

        _isComboInputBuffered = true;
    }

    /// <summary> 지정한 콤보 단계의 공격을 시작한다. </summary>
    private void StartAttack(ComboStep comboStep)
    {
        RotateTowardsNextComboDirection();

        Player.SetCurAttack(this);
        Player.SetSp(-RequiredSp);

        _currentComboStep = comboStep;
        _isComboInputBuffered = false;

        Player.Animator.SetInteger(AnimatorKey.Hash.MeleeComboStep, (int)_currentComboStep);
    }

    /// <summary> 예약된 콤보 입력이 있으면 다음 콤보 공격으로 전환한다. </summary>
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

    /// <summary> 다음 콤보 단계를 반환하고 마지막 단계라면 첫 번째 단계로 돌아간다. </summary>
    private ComboStep GetNextComboStep()
    {
        _currentComboStep++;

        if ((int)_currentComboStep > Utils.GetEnumCount(typeof(ComboStep)))
        {
            _currentComboStep = ComboStep.First;
        }

        return _currentComboStep;
    }

    /// <summary> 현재 공격 범위 안의 적에게 데미지를 적용한다. </summary>
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

        Player.Cam?.ShakeCamera((int)_currentComboStep);

        foreach (Collider collider in colliders)
        {
            if (!collider.TryGetComponent(out IDamageable damageable))
            {
                continue;
            }

            damageable.TakeDamage(GetRandomDamage());
        }
    }

    #endregion ===== 공격 =====

    #region ===== 칼집 =====

    /// <summary> 칼집 상태를 설정한다. </summary>
    public void SetSheathing(bool value)
    {
        _isSheathing = value;
    }

    /// <summary> 칼집 취소 상태를 설정한다. </summary>
    private void SetSheathingCancelled(bool value)
    {
        _isSheatheCancelled = value;
    }

    /// <summary> 현재 칼집 상태를 취소하고 공격 상태를 초기화한다. </summary>
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

    #endregion ===== 칼집 =====

    #region ===== 이펙트 =====

    /// <summary> 슬래시 이펙트의 로컬 위치와 회전을 설정한다. </summary>
    public void SetSlashVFXTransform(Vector3 localPosition, Vector3 localEulerAngles)
    {
        if (_slashVFX == null)
        {
            return;
        }

        Transform slashTransform = _slashVFX.transform;

        slashTransform.localPosition = localPosition;
        slashTransform.localEulerAngles = localEulerAngles;
    }

    /// <summary> 슬래시 이펙트를 재생한다. </summary>
    public void PlaySlashVFX()
    {
        if (_slashVFX == null)
        {
            CPrint.Warning("[PlayerMeleeAttack] 슬래시 파티클을 찾을 수 없습니다.");
            return;
        }

        _slashVFX.Play();
    }

    #endregion ===== 이펙트 =====
}