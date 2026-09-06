using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerCtrl : MonoBehaviour, IDamageable
{
    private CamCtrl _cam;
    public CamCtrl Cam => _cam;

    public Transform Tr => transform;

    private Animator _animator;
    public Animator Animator => _animator;

    private Rigidbody _rigid;
    public Rigidbody Rigid => _rigid;

    private CapsuleCollider _capsuleCollider;

    private int _reactionLayerIndex;


    #region =====트랜스폼=====

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

    #endregion =====트랜스폼=====


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

    [Header("상태")]
    [SerializeField] private LayerMask _groundCheckLayer;
    [SerializeField] private float _groundCheckDistance = 0.1f;

    private PlayerState _state;

    public bool IsGrounded => HasState(PlayerState.Grounded);
    public bool IsDashing => HasState(PlayerState.Dashing);
    public bool IsRunning => HasState(PlayerState.Running);
    public bool IsJumping => HasState(PlayerState.Jumping);
    public bool IsColliding => HasState(PlayerState.Colliding);
    public bool IsDead => HasState(PlayerState.Dead);
    public bool IsHit => HasState(PlayerState.Hit);

    public bool IsMoving => Managers.Input != null && Managers.Input.KeyVecSqrMagnitude > Mathf.Epsilon;


    [Header("피격")]
    [SerializeField] private float _hitInvincibleDuration = 0.5f;
    private float _hitInvincibleTimer;

    public bool IsHitInvincible => _hitInvincibleTimer > 0f;

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
    public float MaxHP => _maxHp;

    private float _hp;
    public float HP => _hp;

    public event Action<float, float> OnHpChanged;


    [Header("SP")]
    [SerializeField] private float _maxSp = 100f;
    public float MaxSP => _maxSp;

    private float _sp;
    public float SP => _sp;

    [Tooltip("SP가 모두 소진된 후 다시 행동할 수 있게 되는 최소 SP")]
    [SerializeField] private float _spRecoveryThreshold = 10f;
    public float SpRecoveryThreshold => _spRecoveryThreshold;

    [Tooltip("초당 SP 회복량")]
    [SerializeField] private float _spRecoveryRate = 30f;

    /// <summary> 최소SP가 충분하여 스태미너를 사용 할 수 있는지 여부 </summary>
    public bool CanUseStamina { get; private set; }

    public event Action<float, float> OnSpChanged;


    [Header("STR")]
    [SerializeField] private float _str = 10;
    public float STR => _str;

    #endregion ===== 스탯 =====


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _reactionLayerIndex = _animator.GetLayerIndex(AnimatorKey.Layer.Reaction);

        _rigid = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        InitializeStat();
    }

    private void Update()
    {
        UpdateHitInvincibility();

        // 지형체크
        bool isGrounded = CheckGroundStatus();
        bool isFalling = !isGrounded && Rigid.velocity.y <= -_fallAnimationMinSpeed;
        // 낙하 애니메이션 재생/정지
        _animator.SetBool(AnimatorKey.Hash.IsFall, isFalling);

        CheckDie();
        RecoverSp();
    }

    private void FixedUpdate()
    {
        ApplyBonusFallGravity();
        FixedUpdateLocomotions();
    }

    private void InitializeStat()
    {
        _hp = _maxHp;
        _sp = _maxSp;

        CanUseStamina = true;
    }

    private void UpdateHitInvincibility()
    {
        if (_hitInvincibleTimer <= 0f)
        {
            return;
        }

        _hitInvincibleTimer -= Time.deltaTime;
    }

    private bool CheckGroundStatus()
    {
        float radius = _capsuleCollider.bounds.extents.x * 0.5f;

        Ray ray = new Ray(transform.position + Vector3.up * radius * 2, Vector3.down);

        bool isGrounded = Physics.SphereCast(ray, radius, radius + _groundCheckDistance, _groundCheckLayer);

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

    private void ApplyBonusFallGravity()
    {
        if (IsGrounded)
        {
            return;
        }

        Vector3 velocity = Rigid.velocity;
        velocity.y -= _bonusFallGravity * Time.fixedDeltaTime;
        velocity.y = Mathf.Max(velocity.y, -_maxFallSpeed);

        Rigid.velocity = velocity;
    }

    public void SetCamera(CamCtrl cam)
    {
        _cam = cam;
    }

    public void TeleportToTarget(Transform target, Action completionAction)
    {
        if (target == null)
        {
            return;
        }

        Managers.UI.OpenLoadingUI(0.3f, openAction: () =>
        {
            _rigid.position = target.position;
            _rigid.rotation = target.rotation;
            _rigid.velocity = Vector3.zero;
            _rigid.angularVelocity = Vector3.zero;

            transform.position = target.position;
            transform.rotation = target.rotation;

            _cam.ResetRotationToTarget(6f);

            completionAction?.Invoke();

            Managers.UI.CloseLoadingUI(0.3f);
        });
    }

    public void PlayHitEffect(Transform attacker)
    {
        Transform hitEffect = Managers.Pool.Get(PoolKey.Path.PlayerHitEffect).transform;
        hitEffect.GetOrAddComponent<LifetimePoolObject>().SetLifetime(0.5f);
        
        // 공격자 방향 계산
        Vector3 direction = (attacker.position - transform.position).normalized;
        // 플레이어 위치에서 공격자 방향으로 아주 조금 이동
        hitEffect.transform.position = transform.position + 
                                       Vector3.up * 1.2f + direction * 0.1f;
        hitEffect.transform.LookAt(attacker);
    }

    
    #region ===== 로코모션(행동) =====

    private void FixedUpdateLocomotions()
    {
        if (IsDead || IsHit)
        {
            return;
        }

        if (_currentAttack == null)
        {
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
    }

    public void SetDefLocomotionBehaviour(int locomotionBehaviourHash)
    {
        _defaultLocomotionHash = locomotionBehaviourHash;
        _currentLocomotionHash = locomotionBehaviourHash;
    }

    public void SetCurLocomotionBehaviour(int locomotionBehaviourHash)
    {
        if (_currentLocomotionHash == _defaultLocomotionHash)
        {
            _currentLocomotionHash = locomotionBehaviourHash;
        }
    }

    public void UnsetCurLocomotionBehaviour(int locomotionBehaviourHash)
    {
        if (_currentLocomotionHash == locomotionBehaviourHash)
        {
            _currentLocomotionHash = _defaultLocomotionHash;
        }
    }

    public void AddLocomotionBehaviour(BaseLocomotionBehaviour locomotionBehaviour)
    {
        if (!_locomotions.Contains(locomotionBehaviour))
        {
            _locomotions.Add(locomotionBehaviour);
        }
    }

    private bool IsCurLocomotionBehaviour(int locomotionBehaviourHash)
    {
        return _currentLocomotionHash == locomotionBehaviourHash;
    }

    #endregion ===== 로코모션(행동) =====


    #region ===== 공격(행동) =====

    public void SetCurAttack(PlayerAttackBehaviour attackBehaviour)
    {
        _currentAttack = attackBehaviour;
    }

    public void UnsetCurAttack()
    {
        _currentAttack = null;
    }

    private void ClearCurAttack()
    {
        if (_currentAttack == null)
        {
            return;
        }

        _currentAttack.Clear();
    }

    #endregion ===== 공격(행동) =====


    #region ===== 데미지/죽음 =====

    public void TakeDamage(float damage)
    {
        if (IsDead || IsHitInvincible)
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

        EnterHit();
    }

    private void EnterHit()
    {
        if (IsDead)
        {
            return;
        }

        // 현재 공격 강제 종료
        ClearCurAttack();

        // 피격 상태
        SetState(PlayerState.Hit);

        // 피격 애니메이션
        int hitHash = UnityEngine.Random.Range(0, 2) == 0 ? AnimatorKey.Hash.Hit1 : AnimatorKey.Hash.Hit2;
        _animator.CrossFade(hitHash, 0.02f, _reactionLayerIndex, 0f);
    }

    private void CheckDie()
    {
        if (_rigid.velocity.y < Mathf.Epsilon && transform.position.y < -10f && !IsDead)
        {
            Die();
        }
    }

    public void Die()
    {
        if (IsDead)
        {
            return;
        }

        UnsetCurAttack();

        SetState(PlayerState.Dead);

        int dieHash = AnimatorKey.Hash.Die;
        _animator.CrossFade(dieHash, 0.02f, _reactionLayerIndex, 0f);

        _rigid.isKinematic = true;
        _capsuleCollider.enabled = false;
        transform.position += Vector3.up * 0.1f;
        
        Managers.Event.RaisePlayerDead();
    }

    public void OnHitAnimationFinished()
    {
        UnsetState(PlayerState.Hit);
    }

    public void OnDeadAnimationEnded()
    {
        Managers.UI.OpenScreen<EndScreen>()?.Open(true);
    }

    #endregion ===== 데미지/죽음 =====


    #region ===== 상태 =====

    public void SetState(PlayerState state)
    {
        _state |= state;
    }

    public void UnsetState(PlayerState state)
    {
        _state &= ~state;
    }

    private bool HasState(PlayerState state)
    {
        return (_state & state) != 0;
    }

    #endregion ===== 상태 =====


    #region ===== 스탯 =====

    public void SetHp(float value)
    {
        float previousHp = _hp;

        _hp = Mathf.Clamp(_hp + value, 0f, _maxHp);

        if (!Mathf.Approximately(previousHp, _hp))
        {
            OnHpChanged?.Invoke(_hp, _maxHp);
        }
    }

    /// <summary> 특정 행동에 필요한 SP가 충분한지 확인 </summary>
    public bool HasEnoughSp(float requiredSp)
    {
        return _sp >= requiredSp && CanUseStamina;
    }

    public void SetSp(float value)
    {
        _sp = Mathf.Clamp(_sp + value, 0f, _maxSp);

        if (_sp <= 0f)
        {
            CanUseStamina = false;
        }
        else if (!CanUseStamina && _sp >= _spRecoveryThreshold)
        {
            CanUseStamina = true;
        }

        OnSpChanged?.Invoke(_sp, _maxSp);
    }

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

        SetSp(_spRecoveryRate * (IsMoving ? 0.5f : 1f) * Time.deltaTime);
    }

    #endregion ===== 스탯 =====
}