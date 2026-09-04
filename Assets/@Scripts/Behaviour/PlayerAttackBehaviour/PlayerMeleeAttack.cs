using UnityEngine;

public class PlayerMeleeAttack : PlayerAttackBehaviour
{
    private enum ComboStep
    {
        First = 1,
        Second,
        Third,
    }

    private ComboStep _currentComboStep;

    // 콤보 입력 여부
    private bool _isComboInputBuffered;

    // 시즈 상태
    private bool _isSheathing;
    private bool _isSheatheCancelled;


    #region ===== 방향 =====

    // 다음 콤보 공격에 사용할 방향
    private Vector3 _nextComboDirection;
    private bool _hasNextComboDirection;

    // FixedUpdate에서 적용할 공격 회전
    private Quaternion _targetAttackRotation;
    private bool _isAttackRotationRequested;

    #endregion


    [Header("공격 판정")]
    [SerializeField] private AttackRange _meleeAttackRange;
    [SerializeField] private LayerMask _meleeLayerMask;

    [Header("이펙트")]
    [SerializeField] private ParticleSystem _slashVFX;


    protected override void OnFixedUpdate()
    {
        ApplyAttackRotation();
    }

    protected override void OnUpdate()
    {
        // 현재 WASD 입력 방향을 계속 저장
        UpdateNextComboDirection();

        if (Managers.Input.MouseDown_Left)
        {
            TryAttack();
        }
    }

    /// <summary> 현재 공격 상태를 초기화한다. 피격, 사망 등 공격을 강제로 취소해야 할 때 사용한다. </summary>
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


    #region ===== 방향 =====

    /// <summary> 현재 입력된 방향을 다음 공격 방향으로 저장한다. 공격 중에도 계속 갱신한다. </summary>
    private void UpdateNextComboDirection()
    {
        Vector3 inputDirection = GetInputDirection();

        if (inputDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        _nextComboDirection = inputDirection.normalized;
        _hasNextComboDirection = true;
    }

    /// <summary> 카메라가 바라보는 방향을 기준으로 현재 입력된 이동 방향을 계산한다. </summary>
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

        Transform camTr = Player.Cam.transform;

        Vector3 forward = camTr.forward;
        Vector3 right = camTr.right;

        // 카메라의 위아래 기울기는 제거
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * input.y + right * input.x;

        return direction.normalized;
    }

    /// <summary> 저장된 방향으로 회전을 요청한다. 실제 회전은 FixedUpdate에서 처리한다. </summary>
    private void RotateTowardsNextComboDirection()
    {
        if (!_hasNextComboDirection)
        {
            return;
        }

        if (_nextComboDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        _targetAttackRotation = Quaternion.LookRotation(_nextComboDirection);
        _isAttackRotationRequested = true;

        _nextComboDirection = Vector3.zero;
        _hasNextComboDirection = false;
    }

    /// <summary> Rigidbody를 통해 공격 방향 회전을 적용한다. </summary>
    private void ApplyAttackRotation()
    {
        if (!_isAttackRotationRequested)
        {
            return;
        }

        Player.Rigid.MoveRotation(_targetAttackRotation);

        _isAttackRotationRequested = false;
    }

    #endregion


    #region ===== 공격 =====

    /// <summary> 현재 상태에 따라 공격을 시작하거나 다음 콤보 공격을 예약한다. </summary>
    private void TryAttack()
    {
        // 현재 기본 로코모션 상태가 아니면 공격 불가
        if (!Player.IsDefaultBehaviour)
        {
            return;
        }

        // 공격 중이 아니라면 첫 공격 시작
        if (!Player.IsAttacking)
        {
            if (!Player.HasEnoughSp(RequiredSp))
            {
                return;
            }

            StartAttack(ComboStep.First);

            return;
        }

        // 이미 다음 콤보 입력이 저장되어 있다면 무시
        if (_isComboInputBuffered)
        {
            return;
        }

        // 다음 콤보 예약
        _isComboInputBuffered = true;
    }

    /// <summary> 공격을 시작한다. 공격 시작 직전에 현재 입력 방향을 회전 요청으로 저장한다. </summary>
    private void StartAttack(ComboStep comboStep)
    {
        RotateTowardsNextComboDirection();

        Player.SetCurAttack(this);
        Player.SetSp(-RequiredSp);

        _currentComboStep = comboStep;

        Player.Animator.SetInteger(AnimatorKey.Hash.MeleeComboStep, (int)_currentComboStep);

        _isComboInputBuffered = false;
    }

    /// <summary> Animation Event에서 호출하며 예약된 다음 콤보가 있으면 다음 공격으로 전환한다. </summary>
    public void TryTransitionCombo()
    {
        if (!_isComboInputBuffered)
        {
            return;
        }

        if (!Player.IsAttacking)
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

    /// <summary> Animation Event에서 호출하며 현재 공격 범위 내 적에게 데미지를 적용한다. </summary>
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

        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(GetRandomDamage());
            }
        }
    }

    #endregion


    #region ===== 시즈 =====

    /// <summary> 시즈 상태를 설정한다. </summary>
    public void SetSheathing(bool value)
    {
        _isSheathing = value;
    }

    /// <summary> 시즈 취소 상태를 설정한다. </summary>
    private void SetSheathingCancelled(bool value)
    {
        _isSheatheCancelled = value;
    }

    /// <summary> 현재 시즈 상태를 취소하고 공격 상태를 초기화한다. </summary>
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

    #endregion


    #region ===== 이펙트 =====

    /// <summary> 슬래시 이펙트의 로컬 위치와 회전을 설정한다. </summary>
    public void SetSlashVFXTransform(Vector3 localPosition, Vector3 localEulerAngles)
    {
        if (_slashVFX == null)
        {
            return;
        }

        Transform tr = _slashVFX.transform;

        tr.localPosition = localPosition;
        tr.localEulerAngles = localEulerAngles;
    }

    /// <summary> 슬래시 이펙트를 재생한다. </summary>
    public void PlaySlashVFX()
    {
        if (_slashVFX == null)
        {
            CPrint.Warning("슬래시 파티클 없음");
            return;
        }

        _slashVFX.Play();
    }

    #endregion
}