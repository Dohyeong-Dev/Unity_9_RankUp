using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary> 플레이어의 이동 상태, 행동, 스탯, 피격 및 사망 처리를 관리한다. </summary>
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerCtrl : MonoBehaviour, IDamageable
{
    #region ===== 참조 =====

    private CamCtrl _cam;
    public CamCtrl Cam => _cam;

    public Transform Tr => transform;

    private Animator _animator;
    public Animator Animator => _animator;

    private Rigidbody _rigid;
    public Rigidbody Rigid => _rigid;

    private CapsuleCollider _capsuleCollider;

    private int _reactionLayerIndex;
    
    #endregion ===== 참조 =====

    #region ===== 설정 =====

    [Header("회전")]
    [Range(0f, 1f)]
    [SerializeField] private float _rotationSlerpFactor = 0.6f;

    public float RotationSlerpFactor => _rotationSlerpFactor;

    [Header("낙하")]
    [Tooltip("기본 중력에 추가로 적용되는 낙하 가속도")]
    [SerializeField] private float _bonusFallGravity = 400f;

    [Tooltip("캐릭터의 최대 낙하 속도")]
    [SerializeField] private float _maxFallSpeed = 30f;

    [Tooltip("이 속도 이상으로 하강할 때 낙하 애니메이션을 재생")]
    [SerializeField] private float _fallAnimationMinSpeed = 2f;

    [Header("상태")]
    [SerializeField] private LayerMask _groundCheckLayer;
    [SerializeField] private float _groundCheckDistance = 0.1f;

    [Header("피격")]
    [SerializeField] private float _hitInvincibleDuration = 0.5f;
    [SerializeField] private GameObject _hitInvincibleEffect;
    
    [SerializeField] private float _hitEffectLifeTime = 0.3f;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    [Flags]
    public enum PlayerState
    {
        Grounded = 1 << 0,
        Dashing = 1 << 1,
        Jumping = 1 << 2,
        Colliding = 1 << 3,
        Dead = 1 << 4,
        Running = 1 << 5,
        Hit = 1 << 6,
    }

    private PlayerState _state;

    private float _hitInvincibleTimer;

    public bool IsGrounded => HasState(PlayerState.Grounded);
    public bool IsDashing => HasState(PlayerState.Dashing);
    public bool IsRunning => HasState(PlayerState.Running);
    public bool IsJumping => HasState(PlayerState.Jumping);
    public bool IsColliding => HasState(PlayerState.Colliding);
    public bool IsDead => HasState(PlayerState.Dead);
    public bool IsHit => HasState(PlayerState.Hit);
    public bool IsHitInvincible => _hitInvincibleTimer > 0f;

    public bool IsMoving => Managers.Input != null && Managers.Input.KeyVecSqrMagnitude > Mathf.Epsilon;

    #endregion ===== 상태 =====

    #region ===== 행동 =====

    private readonly List<BaseLocomotionBehaviour> _locomotions = new();

    private int _defaultLocomotionHash;
    private int _currentLocomotionHash;

    public bool IsDefaultBehaviour => _currentLocomotionHash == _defaultLocomotionHash;

    private PlayerAttackBehaviour _currentAttack;

    public bool IsAttacking => _currentAttack != null;

    #endregion ===== 행동 =====

    #region ===== 스탯 =====

    [Header("HP")]
    [SerializeField] private float _maxHp = 100f;

    private float _hp;

    public float MaxHP => _maxHp;
    public float HP => _hp;

    public event Action<float, float> OnHpChanged;

    [Header("SP")]
    [SerializeField] private float _maxSp = 100f;

    [Tooltip("SP가 모두 소진된 후 다시 행동할 수 있게 되는 최소 SP")]
    [SerializeField] private float _spActionResumeThreshold = 10f;

    [Tooltip("초당 SP 회복량")]
    [SerializeField] private float _spRecoveryRate = 30f;

    private float _sp;

    public float MaxSP => _maxSp;
    public float SP => _sp;
    public float SpActionResumeThreshold => _spActionResumeThreshold;

    private bool _canUseStamina;

    public event Action<float, float> OnSpChanged;

    [Header("STR")]
    [SerializeField] private float _str = 10f;

    public float STR => _str;

    #endregion ===== 스탯 =====

    private void Awake()
    {
        InitializeComponents();
        InitializeStat();
    }

    private void Update()
    {
        UpdateHitInvincibility();
        UpdateGroundAndFallState();

        CheckDie();
        RecoverSp();
    }

    private void FixedUpdate()
    {
        ApplyBonusFallGravity();
        FixedUpdateLocomotions();
    }

    private void OnEnable()
    {
        _hitInvincibleTimer = 0f;

        Managers.Sound.SetListener(transform);
    }

    private void OnDisable()
    {
        ClearCurAttack();
    }

    #region ===== 초기화 =====

    /// <summary> 플레이어가 사용하는 컴포넌트 참조를 초기화한다. </summary>
    private void InitializeComponents()
    {
        _animator = GetComponent<Animator>();
        _rigid = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        _reactionLayerIndex = _animator.GetLayerIndex(AnimatorKey.Layer.Reaction);
    }

    /// <summary> 플레이어의 초기 스탯을 설정한다. </summary>
    private void InitializeStat()
    {
        _hp = _maxHp;
        _sp = _maxSp;
        _canUseStamina = true;
    }

    #endregion ===== 초기화 =====

    #region ===== 카메라/이동 =====

    /// <summary> 플레이어가 사용할 카메라를 설정한다. </summary>
    public void SetCamera(CamCtrl cam)
    {
        _cam = cam;
    }

    /// <summary> 지정된 위치로 플레이어를 즉시 이동시킨다. </summary>
    public void TeleportToTarget(Transform target)
    {
        if (target == null)
        {
            return;
        }

        _rigid.position = target.position;
        _rigid.rotation = target.rotation;
        _rigid.velocity = Vector3.zero;
        _rigid.angularVelocity = Vector3.zero;

        transform.SetPositionAndRotation(target.position, target.rotation);

        _cam?.ResetRotationToTarget(6f);
    }

    /// <summary> 로딩 UI 연출 후 지정된 위치로 플레이어를 이동시키고 완료 콜백을 실행한다. </summary>
    public void TeleportToTargetWithLoading(Transform target, Action completionAction = null)
    {
        if (target == null)
        {
            return;
        }

        Managers.UI.OpenLoadingUI(0.3f, openAction: () =>
        {
            TeleportToTarget(target);

            completionAction?.Invoke();

            Managers.UI.CloseLoadingUI(0.3f);
        });
    }

    /// <summary> 공중에 있을 때 추가 낙하 중력을 적용한다. </summary>
    private void ApplyBonusFallGravity()
    {
        if (IsGrounded)
        {
            return;
        }

        Vector3 velocity = _rigid.velocity;
        velocity.y -= _bonusFallGravity * Time.fixedDeltaTime;
        velocity.y = Mathf.Max(velocity.y, -_maxFallSpeed);

        _rigid.velocity = velocity;
    }

    #endregion ===== 카메라/이동 =====

    #region ===== 로코모션 =====

    /// <summary> 현재 활성화된 로코모션 행동의 FixedUpdate를 실행한다. </summary>
    private void FixedUpdateLocomotions()
    {
        if (IsDead || IsHit || IsAttacking)
        {
            return;
        }

        for (int i = 0; i < _locomotions.Count; i++)
        {
            BaseLocomotionBehaviour locomotion = _locomotions[i];

            if (!locomotion.isActiveAndEnabled)
            {
                continue;
            }

            if (!IsCurLocomotionBehaviour(locomotion.BehaviourHash))
            {
                continue;
            }

            locomotion.OnFixedUpdate();
            break;
        }
    }

    /// <summary> 기본 로코모션 행동을 설정한다. </summary>
    public void SetDefLocomotionBehaviour(int locomotionBehaviourHash)
    {
        _defaultLocomotionHash = locomotionBehaviourHash;
        _currentLocomotionHash = locomotionBehaviourHash;
    }

    /// <summary> 현재 로코모션 행동을 변경한다. </summary>
    public void SetCurLocomotionBehaviour(int locomotionBehaviourHash)
    {
        if (_currentLocomotionHash != _defaultLocomotionHash)
        {
            return;
        }

        _currentLocomotionHash = locomotionBehaviourHash;
    }

    /// <summary> 지정된 로코모션 행동이 현재 행동이면 기본 행동으로 복귀한다. </summary>
    public void UnsetCurLocomotionBehaviour(int locomotionBehaviourHash)
    {
        if (_currentLocomotionHash != locomotionBehaviourHash)
        {
            return;
        }

        _currentLocomotionHash = _defaultLocomotionHash;
    }

    /// <summary> 플레이어가 사용할 로코모션 행동을 등록한다. </summary>
    public void AddLocomotionBehaviour(BaseLocomotionBehaviour locomotionBehaviour)
    {
        if (locomotionBehaviour == null || _locomotions.Contains(locomotionBehaviour))
        {
            return;
        }

        _locomotions.Add(locomotionBehaviour);
    }

    /// <summary> 지정된 로코모션 행동이 현재 행동인지 확인한다. </summary>
    private bool IsCurLocomotionBehaviour(int locomotionBehaviourHash)
    {
        return _currentLocomotionHash == locomotionBehaviourHash;
    }

    #endregion ===== 로코모션 =====

    #region ===== 공격 =====

    /// <summary> 현재 실행 중인 공격 행동을 설정한다. </summary>
    public void SetCurAttack(PlayerAttackBehaviour attackBehaviour)
    {
        _currentAttack = attackBehaviour;
    }

    /// <summary> 현재 공격 행동 참조를 초기화한다. </summary>
    public void UnsetCurAttack()
    {
        _currentAttack = null;
    }

    /// <summary> 현재 공격 행동을 강제로 종료한다. </summary>
    private void ClearCurAttack()
    {
        if (_currentAttack == null)
        {
            return;
        }

        _currentAttack.Clear();
    }
    
    #endregion ===== 공격 =====

    #region ===== 피격 =====

    /// <summary> 피격 무적 시간을 갱신한다. </summary>
    private void UpdateHitInvincibility()
    {
        if (_hitInvincibleTimer <= 0f)
        {
            _hitInvincibleEffect?.gameObject.SetActive(false);
            return;
        }

        if (_hitInvincibleEffect?.gameObject.activeSelf == false)
        {
            _hitInvincibleEffect?.gameObject.SetActive(true);
            Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ElectronicShield, 0.5f);
        }
        
        _hitInvincibleTimer = Mathf.Max(0f, _hitInvincibleTimer - Time.deltaTime);
    }

    /// <summary> 플레이어에게 피해를 적용하고 생존 시 피격 상태로 전환한다. </summary>
    public void TakeDamage(float damage)
    {
        if (IsHitInvincible || IsDashing || IsDead)
        {
            return;
        }

        SetHp(-damage);

        Managers.UI.OpenHitEffect();

        if (_hp <= 0f)
        {
            Die();
            return;
        }

        _hitInvincibleTimer = _hitInvincibleDuration;
        
        ExecuteHit();
    }

    /// <summary> 현재 행동을 중단하고 피격 상태와 애니메이션을 실행한다. </summary>
    private void ExecuteHit()
    {
        ClearCurAttack();

        SetState(PlayerState.Hit);

        int hitHash = UnityEngine.Random.Range(0, 2) == 0 ? AnimatorKey.Hash.Hit1 : AnimatorKey.Hash.Hit2;

        _animator.CrossFade(hitHash, 0.02f, _reactionLayerIndex, 0f);
    }

    /// <summary> 피격 애니메이션 종료 시 피격 상태를 해제한다. </summary>
    public void OnHitAnimationFinished()
    {
        UnsetState(PlayerState.Hit);
    }

    #endregion ===== 피격 =====

    #region ===== 사망 =====

    /// <summary> 플레이어가 낙사 조건에 해당하는지 확인한다. </summary>
    private void CheckDie()
    {
        if (IsDead)
        {
            return;
        }

        if (_rigid.velocity.y >= Mathf.Epsilon || transform.position.y >= -10f)
        {
            return;
        }

        Die();
    }

    /// <summary> 플레이어의 행동을 종료하고 사망 상태로 전환한다. </summary>
    public void Die()
    {
        if (IsDead)
        {
            return;
        }

        Managers.Sound.StopBgm(3f);
        
        UnsetCurAttack();
        SetState(PlayerState.Dead);

        _animator.CrossFade(AnimatorKey.Hash.Die, 0.02f, _reactionLayerIndex,
            0f);

        _rigid.isKinematic = true;
        _capsuleCollider.enabled = false;

        transform.position += Vector3.up * 0.15f;

        Managers.Event.RaisePlayerDead();
    }

    /// <summary> 사망 애니메이션이 종료되면 엔드 화면을 표시한다. </summary>
    public void OnDeadAnimationEnded()
    {
        Managers.UI.OpenScreen<EndScreen>()?.Set(true);
    }

    #endregion ===== 사망 =====

    #region ===== 상태 =====

    /// <summary> 지정된 플레이어 상태를 활성화한다. </summary>
    public void SetState(PlayerState state)
    {
        _state |= state;
    }

    /// <summary> 지정된 플레이어 상태를 비활성화한다. </summary>
    public void UnsetState(PlayerState state)
    {
        _state &= ~state;
    }

    /// <summary> 지정된 상태가 현재 활성화되어 있는지 확인한다. </summary>
    private bool HasState(PlayerState state)
    {
        return (_state & state) != 0;
    }

    /// <summary> 지면 상태와 낙하 애니메이션 상태를 갱신한다. </summary>
    private void UpdateGroundAndFallState()
    {
        bool isGrounded = CheckGroundStatus();
        bool isFalling = !isGrounded && _rigid.velocity.y <= -_fallAnimationMinSpeed;

        _animator.SetBool(AnimatorKey.Hash.IsFall, isFalling);
    }

    /// <summary> 현재 플레이어가 지면에 닿아 있는지 확인한다. </summary>
    private bool CheckGroundStatus()
    {
        float radius = _capsuleCollider.bounds.extents.x * 0.5f;
        Vector3 origin = transform.position + Vector3.up * radius * 2f;

        bool isGrounded = Physics.SphereCast(origin, radius, Vector3.down, out _,
            radius + _groundCheckDistance, _groundCheckLayer);

        if (isGrounded)
        {
            SetState(PlayerState.Grounded);
        }
        else
        {
            UnsetState(PlayerState.Grounded);
        }

        return isGrounded;
    }
    
    #endregion ===== 상태 =====

    #region ===== 스탯 =====

    /// <summary> HP를 변경하고 값이 달라졌으면 HP 변경 이벤트를 호출한다. </summary>
    public void SetHp(float value)
    {
        float previousHp = _hp;
        _hp = Mathf.Clamp(_hp + value, 0f, _maxHp);

        if (!Mathf.Approximately(previousHp, _hp))
        {
            OnHpChanged?.Invoke(_hp, _maxHp);
        }
    }

    /// <summary> 특정 행동에 필요한 SP를 사용할 수 있는지 확인한다. </summary>
    public bool HasEnoughSp(float requiredSp)
    {
        if (_sp < requiredSp)
        {
            _canUseStamina = false;
        }

        return _sp >= requiredSp && _canUseStamina;
    }

    /// <summary> SP를 변경하고 현재 스태미너 사용 가능 상태를 갱신한다. </summary>
    public void SetSp(float value)
    {
        _sp = Mathf.Clamp(_sp + value, 0f, _maxSp);

        if (_sp <= 0f)
        {
            _canUseStamina = false;
        }
        else if (!_canUseStamina && _sp >= _spActionResumeThreshold)
        {
            _canUseStamina = true;
        }

        OnSpChanged?.Invoke(_sp, _maxSp);
    }

    /// <summary> 현재 상태에 따라 SP를 회복한다. </summary>
    private void RecoverSp()
    {
        if (_sp >= _maxSp)
        {
            return;
        }

        if (IsDashing || IsRunning || IsAttacking || IsDead)
        {
            return;
        }

        float recoveryMultiplier = IsMoving ? 0.5f : 1f;
        SetSp(_spRecoveryRate * recoveryMultiplier * Time.deltaTime);
    }

    #endregion ===== 스탯 =====

    #region ===== 피격 이펙트 ======

    /// <summary> 공격자 방향에 피격 이펙트를 생성한다. </summary>
    public void PlayHitEffect(Transform attacker)
    {
        if (attacker == null)
        {
            return;
        }

        Vector3 direction = (attacker.position - transform.position).normalized;
        Vector3 hitPosition = transform.position + Vector3.up * 1.2f + direction * 0.3f;

        PlayHitEffect(hitPosition, attacker.position);
    }

    /// <summary> 지정된 피격 위치에 피격 이펙트를 생성한다. </summary>
    public void PlayHitEffect(Vector3 hitPosition)
    {
        Vector3 lookPosition = transform.position;
        PlayHitEffect(hitPosition, lookPosition);
    }

    /// <summary> 지정된 위치와 방향으로 피격 이펙트를 생성한다. </summary>
    public void PlayHitEffect(Vector3 hitPosition, Vector3 lookPosition)
    {
        if (IsHitInvincible || IsDead || IsDashing)
        {
            return;
        }
        
        PoolObj poolObject = Managers.Pool.Get(PoolKey.Path.PlayerHitEffect);

        if (poolObject == null)
        {
            return;
        }

        Transform hitEffect = poolObject.transform;
        hitEffect.position = hitPosition;

        Vector3 direction = lookPosition - hitPosition;

        if (direction.sqrMagnitude > Mathf.Epsilon)
        {
            hitEffect.rotation = Quaternion.LookRotation(direction);
        }

        hitEffect.GetOrAddComponent<LifetimePoolObject>().SetLifetime(_hitEffectLifeTime);
    }

    #endregion ===== 피격 이펙트 ======
}