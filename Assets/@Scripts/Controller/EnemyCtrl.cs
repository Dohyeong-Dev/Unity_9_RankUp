using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyCtrl : MonoBehaviour, IDamageable
{
    private PlayerCtrl _player;

    private NavMeshAgent _navMeshAgent;
    public NavMeshAgent NavMeshAgent => _navMeshAgent;
    private Animator _animator;
    public Animator Animator => _animator;
    private Rigidbody _rigid;
    private Collider _collider;

    private Tween _dissolveTween;
    private Tween _knockbackTween;

    // 스폰장소
    private Vector3 _spawnPosition;
    public Vector3 SpawnPosition => _spawnPosition;
    private Quaternion _spawnRotation;
    public Quaternion SpawnRotation => _spawnRotation;


    [Header("이동")]
    [SerializeField] private float _walkSpeed = 1.5f;
    public float WalkSpeed => _walkSpeed;
    [SerializeField] private float _runSpeed = 4f;
    public float RunSpeed => _runSpeed;
    [SerializeField] private float _runSpeedVariance = 1f;
    public float RunSpeedVariance => _runSpeedVariance;
    [SerializeField] private float _rotationSpeed = 360f;
    public float RotationSpeed => _rotationSpeed;
    [SerializeField] private float _chaseStoppingDistance = 2f;
    public float ChaseStoppingDistance => _chaseStoppingDistance;

    [Header("넉백")]
    [SerializeField] private float _knockbackDistance = 1f;
    [SerializeField] private float _knockbackDuration = 0.15f;
    [SerializeField] private Ease _knockbackEase = Ease.OutQuad;

    [Header("넉백 충돌 체크")]
    [SerializeField] private LayerMask _knockbackCollisionLayer;
    [SerializeField] private float _knockbackRadius = 0.3f;
    [SerializeField] private float _knockbackCollisionOffset = 0.05f;

    [Header("이펙트")]
    [SerializeField] private ParticleSystem _hitEffect;

    [Header("디졸브")]
    [SerializeField] private float _dissolveDuration = 0.5f;
    [SerializeField] private Ease _dissolveEase = Ease.InOutQuad;
    private readonly List<Material> _materials = new();
    private float _dissolveValue;

    [Header("애니메이션")]
    [SerializeField] private float _animationSpeedMultiplier = 0.25f;


    #region ===== 타겟 =====

    private FieldOfView _fieldOfView;

    private Transform _combatTarget;

    private float _combatTargetTimer;

    [Header("전투 타겟")]
    [Tooltip("적이 플레이어를 마지막으로 인식한 후 전투 타겟을 유지하는 시간")]
    [SerializeField] private float _combatTargetDuration = 3f;
    [Tooltip("전투 타겟이 이 거리 안에 있으면 전투 타겟 유지 시간이 초기화되는 거리")]
    [SerializeField] private float _combatRetentionDistance = 8f;

    public Transform Target
    {
        get
        {
            if (_combatTarget != null && _combatTargetTimer > 0f)
            {
                return _combatTarget;
            }

            return _fieldOfView != null ? _fieldOfView.CurrentTarget : null;
        }
    }

    #endregion ===== 타겟 =====


    #region ===== 상태 머신 =====

    private readonly EnemyStateMachine _stateMachine = new();

    public enum EnemyState
    {
        None = 0,
        Dead = 1 << 0,
    }

    private EnemyState _state;

    public bool IsDead => HasState(EnemyState.Dead);

    #endregion ===== 상태 머신 =====


    #region ===== 스탯 =====

    [Header("HP")]
    [SerializeField] private float _maxHp = 100f;
    public float MaxHP => _maxHp;

    private float _hp;
    public float HP => _hp;

    public event Action<float, float> OnHpChanged;

    #endregion ===== 스탯 =====


    private void Awake()
    {
        if (Managers.Scene.TryGetCurrentScene(out GameScene gameScene))
        {
            _player = gameScene.Player;
        }

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _rigid = GetComponent<Rigidbody>();
        _fieldOfView = GetComponent<FieldOfView>();
        _collider = GetComponent<Collider>();

        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;

        InitializeMaterials();
        InitializeStateMachine();

        ResetEnemy();
    }

    private void Update()
    {
        if (IsDead)
        {
            return;
        }

        UpdateCombatTarget();

        _stateMachine.UpdateCurrentState(Time.deltaTime);
    }

    private void OnDestroy()
    {
        _dissolveTween?.Kill();
        _knockbackTween?.Kill();
    }

    private void InitializeStateMachine()
    {
        _stateMachine.RegisterState(new IdleState(_stateMachine, this));
        _stateMachine.RegisterState(new ChaseState(_stateMachine, this));
        _stateMachine.RegisterState(new ReturnState(_stateMachine, this));
        _stateMachine.RegisterState(new HitState(_stateMachine, this));
    }

    public void ResetEnemy()
    {
        _dissolveTween?.Kill();
        _dissolveTween = null;

        _knockbackTween?.Kill();
        _knockbackTween = null;

        _state = EnemyState.None;

        _combatTarget = null;
        _combatTargetTimer = 0f;

        _hp = _maxHp;

        _collider.enabled = true;

        if (_rigid != null)
        {
            _rigid.isKinematic = true;
            _rigid.velocity = Vector3.zero;
            _rigid.angularVelocity = Vector3.zero;
        }

        if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.ResetPath();
            _navMeshAgent.velocity = Vector3.zero;
        }

        SetDissolveValue(0f);

        if (_animator != null)
        {
            _animator.SetFloat(AnimatorKey.Hash.Speed, 0f);
        }
    }

    public void Spawn(Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            return;
        }

        _dissolveTween?.Kill();
        _dissolveTween = null;

        _spawnPosition = spawnPoint.position;
        _spawnRotation = spawnPoint.rotation;

        transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);

        if (!_navMeshAgent.enabled)
        {
            _navMeshAgent.enabled = true;
        }

        if (_navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.Warp(_spawnPosition);
        }

        ResetEnemy();

        SetDissolveValue(1f);
        Dissolve(false);

        _stateMachine.ChangeState<IdleState>();

        OnHpChanged?.Invoke(_hp, _maxHp);
    }

    private void PlayHitEffect()
    {
        if (_hitEffect == null)
        {
            CPrint.Warning("HitEffect not found!");
            return;
        }

        _hitEffect.transform.parent.LookAt(_player.transform);
        _hitEffect.Play();
    }

    public void SetHp(float value)
    {
        float previousHp = _hp;

        _hp = Mathf.Clamp(_hp + value, 0f, _maxHp);

        if (!Mathf.Approximately(previousHp, _hp))
        {
            OnHpChanged?.Invoke(_hp, _maxHp);
        }

        if (_hp <= 0f)
        {
            Die();
        }
    }

    private void ReturnToPool()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        StopMovement();

        PoolObj poolObj = GetComponent<PoolObj>();

        if (poolObj != null)
        {
            Managers.Pool.Return(poolObj);
        }
    }
    
    
    #region ===== 타겟 =====

    /// <summary> 전투 타겟이 설정된 거리 안에 있으면 설정된 전투 타겟 유지 시간으로 초기화시킵니다. </summary>
    private void UpdateCombatTarget()
    {
        if (_combatTarget == null)
        {
            return;
        }

        float sqrDistance = (_combatTarget.position - transform.position).sqrMagnitude;

        if (sqrDistance <= _combatRetentionDistance * _combatRetentionDistance)
        {
            _combatTargetTimer = _combatTargetDuration;
            return;
        }

        _combatTargetTimer -= Time.deltaTime;

        if (_combatTargetTimer <= 0f)
        {
            _combatTargetTimer = 0f;
            _combatTarget = null;
        }
    }

    /// <summary> 피해를 받았을 때 타겟을 시야각과 </summary>
    private void SetCombatTarget(Transform target)
    {
        if (target == null)
        {
            return;
        }

        _combatTarget = target;
        _combatTargetTimer = _combatTargetDuration;
    }

    #endregion ===== 타겟 =====


    #region ===== 이동/내브매쉬 =====

    public void MoveTo(Vector3 destination, float moveSpeed)
    {
        if (IsDead)
        {
            return;
        }

        if (_navMeshAgent == null || !_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh)
        {
            return;
        }

        _navMeshAgent.isStopped = false;
        _navMeshAgent.speed = moveSpeed;
        _navMeshAgent.SetDestination(destination);
    }

    public void RotateTowards(Vector3 targetPosition, float rotationSpeed, float deltaTime)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
            rotationSpeed * deltaTime);
    }

    public void UpdateMovementAnimation()
    {
        if (_navMeshAgent == null || !_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh)
        {
            SetAnimationMoveSpeed(0f);
            return;
        }

        float currentSpeed = _navMeshAgent.velocity.magnitude;

        if (currentSpeed <= 0.01f)
        {
            SetAnimationMoveSpeed(0f);
            return;
        }

        SetAnimationMoveSpeed(currentSpeed);
    }

    public void SetAnimationMoveSpeed(float moveSpeed)
    {
        if (_animator == null)
        {
            return;
        }

        float animationSpeed = moveSpeed * _animationSpeedMultiplier;

        _animator.SetFloat(AnimatorKey.Hash.Speed, animationSpeed);
    }

    public void StopMovement()
    {
        if (_navMeshAgent == null || !_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh)
        {
            SetAnimationMoveSpeed(0f);
            return;
        }

        _navMeshAgent.isStopped = true;
        _navMeshAgent.ResetPath();
        _navMeshAgent.velocity = Vector3.zero;

        SetAnimationMoveSpeed(0f);
    }

    #endregion ===== 이동/내브매쉬 =====


    #region ===== 데미지/죽음 =====

    public void TakeDamage(float damage)
    {
        if (IsDead)
        {
            return;
        }

        SetCombatTarget(_player != null ? _player.transform : null);

        SetHp(-damage);

        PlayHitEffect();
        
        if (IsDead)
        {
            return;
        }

        ApplyKnockback(_player != null ? _player.transform.position : transform.position);

        if (_stateMachine.IsCurrentState<HitState>())
        {
            _stateMachine.RestartCurrentState();
        }
        else
        {
            _stateMachine.ChangeState<HitState>();
        }
    }

    public void OnHitAnimationFinished()
    {
        if (IsDead)
        {
            return;
        }

        if (_stateMachine.TryGetCurrentState<HitState>(out var hitState))
        {
            hitState.SetAnimationFinished();
        }
    }

    public void Die()
    {
        if (IsDead)
        {
            return;
        }

        SetState(EnemyState.Dead);

        _knockbackTween?.Kill();
        _knockbackTween = null;

        if (_rigid != null)
        {
            _rigid.velocity = Vector3.zero;
            _rigid.angularVelocity = Vector3.zero;
            _rigid.isKinematic = true;
        }

        StopMovement();

        _combatTarget = null;
        _combatTargetTimer = 0f;

        _collider.enabled = false;

        if (_animator != null)
        {
            _animator.SetFloat(AnimatorKey.Hash.Speed, 0f);
            _animator.SetTrigger(AnimatorKey.Hash.DoHit);
        }

        if (_navMeshAgent != null && _navMeshAgent.enabled)
        {
            _navMeshAgent.enabled = false;
        }

        _dissolveTween?.Kill();
        _dissolveTween = null;

        Dissolve(true);
    }

    #endregion ===== 데미지/죽음 =====


    #region ===== 넉백 =====

    private void ApplyKnockback(Vector3 attackerPosition)
    {
        if (IsDead)
        {
            return;
        }

        Vector3 direction = transform.position - attackerPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            direction = -transform.forward;
        }

        direction.Normalize();

        if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.ResetPath();
        }

        _knockbackTween?.Kill();
        _knockbackTween = null;

        float moveDistance = GetKnockbackDistance(direction);

        if (moveDistance <= Mathf.Epsilon)
        {
            return;
        }

        Vector3 targetPosition = transform.position + direction * moveDistance;

        _knockbackTween = transform.DOMove(targetPosition, _knockbackDuration).SetEase(_knockbackEase).OnComplete(() =>
        {
            if (IsDead)
            {
                return;
            }

            if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.Warp(transform.position);
                _navMeshAgent.isStopped = false;
            }

            _knockbackTween = null;
        });
    }

    private float GetKnockbackDistance(Vector3 direction)
    {
        if (_knockbackCollisionLayer == 0)
        {
            return _knockbackDistance;
        }

        Vector3 origin = transform.position + Vector3.up * _knockbackRadius;
        RaycastHit[] hits = Physics.SphereCastAll(origin, _knockbackRadius, direction, _knockbackDistance,
            _knockbackCollisionLayer, QueryTriggerInteraction.Ignore);

        if (hits.Length == 0)
        {
            return _knockbackDistance;
        }

        float closestDistance = _knockbackDistance;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            float hitDistance = Mathf.Max(0f, hit.distance - _knockbackCollisionOffset);
            closestDistance = Mathf.Min(closestDistance, hitDistance);
        }

        return closestDistance;
    }

    #endregion ===== 넉백 =====

    
    #region ===== 상태 =====

    public void SetState(EnemyState state)
    {
        _state |= state;
    }

    public void UnsetState(EnemyState state)
    {
        _state &= ~state;
    }

    private bool HasState(EnemyState state)
    {
        return (_state & state) != 0;
    }

    #endregion ===== 상태 =====


    #region ===== 디졸브 =====

    private void InitializeMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;

            foreach (Material material in materials)
            {
                if (material.HasProperty("_Dissolve"))
                {
                    _materials.Add(material);
                }
            }
        }
    }

    private void SetDissolveValue(float value)
    {
        _dissolveValue = value;

        foreach (Material material in _materials)
        {
            material.SetFloat("_Dissolve", value);
        }
    }

    private void Dissolve(bool isDissolve)
    {
        float targetValue = isDissolve ? 1f : 0f;

        _dissolveTween?.Kill();

        _dissolveTween = DOTween.To(() => _dissolveValue, SetDissolveValue, targetValue, _dissolveDuration)
            .SetEase(_dissolveEase);

        if (isDissolve)
        {
            _dissolveTween.OnComplete(ReturnToPool);
        }
    }

    #endregion ===== 디졸브 =====
}