using System;
using System.Collections.Generic;
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
    }

    [Header("상태")]
    [SerializeField] private LayerMask _groundCheckLayer;
    [SerializeField] private float _groundCheckDistance = 0.1f;

    private PlayerState _state;

    public bool IsGrounded => HasState(PlayerState.Grounded);
    public bool IsDashing => HasState(PlayerState.Dashing);
    public bool IsJumping => HasState(PlayerState.Jumping);
    public bool IsColliding => HasState(PlayerState.Colliding);
    public bool IsDead => HasState(PlayerState.Dead);

    public bool IsMoving => Managers.Input != null && Managers.Input.KeyVecSqrMagnitude > Mathf.Epsilon;

    #endregion ===== 상태 =====


    #region ===== 행동 =====

    private readonly List<BaseLocomotionBehaviour> _locomotions = new();

    private int _defaultLocomotionBehaviourHash;
    private int _currentLocomotionBehaviourHash;

    public bool IsDefaultBehaviour => _currentLocomotionBehaviourHash == _defaultLocomotionBehaviourHash;

    private BaseAttackBehaviour _curAttack;
    public bool IsAttacking => _curAttack != null;

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
        _rigid = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        InitializeStat();
    }

    private void InitializeStat()
    {
        _hp = _maxHp;
        _sp = _maxSp;

        CanUseStamina = true;
    }

    private void Update()
    {
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

    
    #region ===== 로코모션 =====

    private void FixedUpdateLocomotions()
    {
        if (IsDead)
        {
            return;
        }

        if (_curAttack == null)
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
        _defaultLocomotionBehaviourHash = locomotionBehaviourHash;
        _currentLocomotionBehaviourHash = locomotionBehaviourHash;
    }

    public void SetCurLocomotionBehaviour(int locomotionBehaviourHash)
    {
        if (_currentLocomotionBehaviourHash == _defaultLocomotionBehaviourHash)
        {
            _currentLocomotionBehaviourHash = locomotionBehaviourHash;
        }
    }

    public void UnsetCurLocomotionBehaviour(int locomotionBehaviourHash)
    {
        if (_currentLocomotionBehaviourHash == locomotionBehaviourHash)
        {
            _currentLocomotionBehaviourHash = _defaultLocomotionBehaviourHash;
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
        return _currentLocomotionBehaviourHash == locomotionBehaviourHash;
    }

    #endregion ===== 로코모션 =====


    #region ===== 공격 =====

    public void SetCurAttack(BaseAttackBehaviour attackBehaviour)
    {
        _curAttack = attackBehaviour;
    }

    public void UnsetCurAttack()
    {
        _curAttack = null;
    }

    #endregion ===== 공격 =====


    #region ===== 데미지/죽음 =====

    public void TakeDamage(float damage)
    {
        if (IsDead)
        {
            return;
        }

        SetHp(-damage);
        Managers.UI.OpenHitEffect();

        if (_hp <= 0)
        {
            Die();
        }
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

        SetState(PlayerState.Dead);

        _animator.SetTrigger(AnimatorKey.Hash.DoDie);
    }

    public void OnDeadAnimationEnd()
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
        float previousSp = _sp;

        _sp = Mathf.Clamp(_sp + value, 0f, _maxSp);

        if (_sp <= 0f)
        {
            CanUseStamina = false;
        }
        else if (!CanUseStamina && _sp >= _spRecoveryThreshold)
        {
            CanUseStamina = true;
        }

        if (!Mathf.Approximately(previousSp, _sp))
        {
            OnSpChanged?.Invoke(_sp, _maxSp);
        }
    }

    private void RecoverSp()
    {
        if (_sp >= _maxSp)
        {
            return;
        }

        if (IsMoving || IsAttacking)
        {
            return;
        }

        SetSp(_spRecoveryRate * Time.deltaTime);
    }

    #endregion ===== 스탯 =====
}