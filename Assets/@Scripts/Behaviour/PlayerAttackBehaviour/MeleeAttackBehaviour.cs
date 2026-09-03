using UnityEngine;

public class MeleeAttackBehaviour : BaseAttackBehaviour
{
    private enum ComboStep
    {
        First = 1,
        Second,
        Third,
    }
    private ComboStep _currentComboStep;

    private bool _isComboInputBuffered;
    private bool _isSheathing;
    private bool _isSheatheCancelled;

    // 공격 중 방향전환
    private Vector3 _nextComboDirection;
    private bool _hasNextComboDirection;
    

    [Header("공격 판정")]
    [SerializeField] private AttackRange _meleeAttackRange;
    [SerializeField] private LayerMask _meleeLayerMask;
    
    [Header("이펙트")]
    [SerializeField] private ParticleSystem _slashVFX;


    protected override void OnUpdate()
    {
        UpdateNextComboDirection();

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
        
        _nextComboDirection = Vector3.zero;
        _hasNextComboDirection = false;
        SetSheathing(false);
        SetSheathingCancelled(false);

        Player.Animator.SetInteger(AnimatorKey.Hash.MeleeComboStep, 0);
    }


    #region ===== 방향 =====

    private void UpdateNextComboDirection()
    {
        if (!Player.IsAttacking)
        {
            return;
        }

        Vector3 inputDirection = GetInputDirection();

        if (inputDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        _nextComboDirection = inputDirection.normalized;
        _hasNextComboDirection = true;
    }

    /// <summary> 카메라 보는 방향 기준으로 입력된 방향을 구한다. </summary>
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

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * input.y + right * input.x;

        return direction.normalized;
    }

    private void RotateTowardsNextComboDirection()
    {
        if (!_hasNextComboDirection)
        {
            return;
        }
        
        Quaternion targetRotation = Quaternion.LookRotation(_nextComboDirection);
        Player.Tr.rotation = targetRotation;

        _nextComboDirection = Vector3.zero;
        _hasNextComboDirection = false;
    }

    #endregion ===== 방향 =====


    #region ===== 공격 =====

    private void TryAttack()
    {
        if (!Player.IsDefaultBehaviour)
        {
            return;
        }

        if (!Player.IsAttacking)
        {
            if (Player.HasEnoughSp(RequiredSp))
            {
                StartAttack(ComboStep.First);
            }

            return;
        }

        if (_isComboInputBuffered)
        {
            return;
        }

        _isComboInputBuffered = true;
    }

    private void StartAttack(ComboStep comboStep)
    {
        if (Player.IsAttacking)
        {
            RotateTowardsNextComboDirection();
        }

        Player.SetCurAttack(this);
        Player.SetSp(-RequiredSp);

        _currentComboStep = comboStep;
        Player.Animator.SetInteger(AnimatorKey.Hash.MeleeComboStep, (int)_currentComboStep);

        _isComboInputBuffered = false;
    }

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

    private ComboStep GetNextComboStep()
    {
        _currentComboStep++;

        if ((int)_currentComboStep > Utils.GetEnumCount(typeof(ComboStep)))
        {
            _currentComboStep = ComboStep.First;
        }

        return _currentComboStep;
    }

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

        Player.Cam.ShakeCamera((int)_currentComboStep);

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

    public void SetSheathing(bool value)
    {
        _isSheathing = value;
    }

    private void SetSheathingCancelled(bool value)
    {
        _isSheatheCancelled = value;
    }

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