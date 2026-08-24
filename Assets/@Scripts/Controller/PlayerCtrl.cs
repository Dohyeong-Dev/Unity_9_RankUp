using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour, IDamageable
{
    [Flags]
    public enum PlayerState
    {
        Grounded = 1 << 0,
        Dashing = 1 << 1,
        Jumping = 1 << 2,
        Colliding = 1 << 3,
        Dead = 1 << 4,
    }

    private Camera _camera;
    public Camera Cam => _camera;

    public Transform Tr => transform;

    private Animator _animator;
    public Animator Animator => _animator;
    private Rigidbody _rigid;
    public Rigidbody Rigid => _rigid;
    private CapsuleCollider _capsuleCollider;
    public CapsuleCollider CapsuleCollider => _capsuleCollider;
    
    // 회전
    [Header("회전")]
    [Range(0f, 1f)]
    [SerializeField] private float _rotationSlerpFactor;
    public float RotationSlerpFactor => _rotationSlerpFactor;
    private Vector3 _lastDirection;

    // 행동
    private readonly List<BaseLocomotionBehaviour> _locomotions = new();
    private int _defaultLocomotionBehaviourHash;
    private int _currentLocomotionBehaviourHash;
    public bool IsDefaultBehaviour => _currentLocomotionBehaviourHash == _defaultLocomotionBehaviourHash;

    #region Stat
    
    [Header("스탯")]
    // HP
    [SerializeField] private float _maxHp = 100f;
    public float MaxHP => _maxHp;
    private float _hp;
    public float HP => _hp;
    public event Action<float, float> OnHpChanged;
    // SP
    [SerializeField] private float _maxSp = 100f;
    public float MaxSP => _maxSp;
    private float _sp;
    public float SP => _sp;
    [Tooltip("SP가 모두 소진된 후 다시 행동할 수 있게 되는 최소 SP")]
    [SerializeField] private float _spRecoveryThreshold = 10f;
    [Tooltip("초당 SP 회복량")]
    [SerializeField] private float _spRecoveryRate = 30f;
    /// <summary> SP가 충분하여 스태미너를 사용하는 행동을 할 수 있는지 여부  </summary>
    public bool CanUseStamina { get; private set; }
    public event Action<float, float> OnSpChanged;
    // STR
    [SerializeField] private float _str = 10;
    public float STR => _str;
    #endregion
    
    #region State

    private PlayerState _state;

    public bool IsGrounded => HasState(PlayerState.Grounded);
    public bool IsDashing => HasState(PlayerState.Dashing);
    public bool IsJumping => HasState(PlayerState.Jumping);
    public bool IsColliding => HasState(PlayerState.Colliding);
    public bool IsDead => HasState(PlayerState.Dead); // IsAlive가 0이 되었을 때 True
    
    public bool CanControl => Managers.UI.CurrentHUD?.IsInputEnabled ?? false;

    public bool IsMoving
    {
        get
        {
            if (Managers.Input == null)
            {
                return false;
            }

            return Managers.Input.KeyVecMagnitude > Mathf.Epsilon;
        }
    }

    private bool _canDash;
    public bool CanDash => _canDash;

    #endregion

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigid = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        _camera = Camera.main;

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
        _animator.SetBool(AnimatorKey.Hash.IsGround, CheckGroundStatus());

        CheckDie();
        RecoverSp();
    }

    private void FixedUpdate()
    {
        UpdateBehaviours();
    }

    #region Behaviour

    private void UpdateBehaviours()
    {
        if (IsDead)
        {
            return;
        }

        bool isPlayingLocomotion = false;

        for (int i = 0; i < _locomotions.Count; i++)
        {
            BaseLocomotionBehaviour locomotion = _locomotions[i];

            if (!locomotion.isActiveAndEnabled)
            {
                continue;
            }

            if (_currentLocomotionBehaviourHash != locomotion.BehaviourHash)
            {
                continue;
            }

            isPlayingLocomotion = true;

            locomotion.OnFixedUpdate();

            break;
        }

        if (!isPlayingLocomotion)
        {
            Reposit();
        }
    }

    // 마지막으로 바라보던 방향으로 플레이어를 회전
    private void Reposit()
    {
        if (_lastDirection == Vector3.zero)
        {
            return;
        }

        Vector3 viewDir = _lastDirection;
        viewDir.y = 0f;
        if (viewDir.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(viewDir);
        Quaternion viewRotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSlerpFactor);

        _rigid.MoveRotation(viewRotation);
    }

    #endregion

    #region Damage

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

    public void Die()
    {
        if (IsDead)
        {
            return;
        }
        
        SetState(PlayerState.Dead);

        _animator.SetTrigger(AnimatorKey.Hash.DoDie);
    }

    public void OnDeathAnimationEnd()
    {
        Managers.UI.OpenScreen<EndScreen>()?.Open(true);
    }
    
    #endregion

    #region LocomotionBehaviour

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

    public void AddBehaviour(BaseLocomotionBehaviour locomotionBehaviour)
    {
        if (!_locomotions.Contains(locomotionBehaviour))
        {
            _locomotions.Add(locomotionBehaviour);
        }
    }

    public bool IsCurrentBehaviour(int locomotionBehaviourHash)
    {
        return _currentLocomotionBehaviourHash == locomotionBehaviourHash;
    }

    #endregion

    #region Check

    public bool CheckGroundStatus()
    {
        float radius = _capsuleCollider.bounds.extents.x * 0.5f;

        Ray ray = new Ray(transform.position + Vector3.up * radius * 2f, Vector3.down);

        bool isGrounded = Physics.SphereCast(ray, radius, radius + 0.1f, LayerKey.Mask.Floor);
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

    private void CheckDie()
    {
        if (_rigid.velocity.y < Mathf.Epsilon && transform.position.y < -10f && !IsDead)
        {
            Die();
        }
    }

    #endregion

    #region State

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

    #endregion

    #region Stat

    public void SetHp(float value)
    {
        float previousHp = _hp;

        _hp = Mathf.Clamp(_hp + value, 0f, _maxHp);

        if (!Mathf.Approximately(previousHp, _hp))
        {
            OnHpChanged?.Invoke(_hp, _maxHp);
        }
    }

    public void SetSp(float value)
    {
        float previousSp = _sp;

        _sp = Mathf.Clamp(_sp + value, 0f, _maxSp);

        // SP가 모두 소진되면 스태미너 사용 행동 차단
        if (_sp <= 0f)
        {
            CanUseStamina = false;
        }
        // 일정량 이상 회복되면 다시 사용 가능
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
        // 이미 최대 SP라면 회복할 필요 없음
        if (_sp >= _maxSp)
        {
            return;
        }

        // 이동 중에는 SP를 회복하지 않음
        if (IsMoving)
        {
            return;
        }

        SetSp(_spRecoveryRate * Time.deltaTime);
    }

    /// <summary> 특정 행동에 필요한 SP가 충분한지 확인  </summary>
    public bool HasEnoughSp(float requiredSp)
    {
        return _sp >= requiredSp && CanUseStamina;
    }

    #endregion

    #region Set

    public void SetCamera(Camera cam)
    {
        _camera = cam;
    }

    public void SetCanDash(bool canDash)
    {
        _canDash = canDash;
    }

    public void SetLastDirection(Vector3 lastDirection)
    {
        _lastDirection = lastDirection;
    }

    #endregion
}