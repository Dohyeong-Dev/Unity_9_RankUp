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
    private static readonly int DissolveProperty = Shader.PropertyToID("_Dissolve");

    #region ===== 컴포넌트 =====

    private PlayerCtrl _player;

    protected NavMeshAgent _navMeshAgent;
    protected Animator _animator;
    private Rigidbody _rigid;
    protected Collider _collider;
    protected FieldOfView _fieldOfView;

    public NavMeshAgent NavMeshAgent => _navMeshAgent;
    public Animator Animator => _animator;

    #endregion ===== 컴포넌트 =====

    #region ===== 스폰 =====

    private Vector3 _spawnPosition;
    public Vector3 SpawnPosition => _spawnPosition;

    private Quaternion _spawnRotation;
    public Quaternion SpawnRotation => _spawnRotation;

    #endregion ===== 스폰 =====

    #region ===== 이동 =====

    [Header("이동")]
    [SerializeField] private float _walkSpeed = 1.5f;
    [SerializeField] private float _runSpeed = 4f;
    [SerializeField] private float _runSpeedVariance = 1f;
    [SerializeField] private float _rotationSpeed = 360f;
    [SerializeField] private float _defaultChaseStoppingDistance = 1f;

    [Header("애니메이션")]
    [Tooltip("실제 이동 속도를 애니메이션 속도 값으로 변환할 때 사용하는 배율")]
    [SerializeField] private float _animationSpeedMultiplier = 0.2f;

    public float WalkSpeed => _walkSpeed;
    public float RunSpeed => _runSpeed;
    public float RunSpeedVariance => _runSpeedVariance;
    public float RotationSpeed => _rotationSpeed;

    public float ChaseStoppingDistance => CurrentAttackBehaviour != null
        ? CurrentAttackBehaviour.AttackableDistance
        : _defaultChaseStoppingDistance;

    #endregion ===== 이동 =====

    #region ===== 타겟 =====

    [Header("전투 타겟")]
    [Tooltip("적이 플레이어를 마지막으로 인식한 후 전투 타겟을 유지하는 시간")]
    [SerializeField] private float _combatTargetDuration = 3f;

    [Tooltip("전투 타겟이 이 거리 안에 있으면 타겟 유지 시간이 초기화되는 거리")]
    [SerializeField] private float _combatRetentionDistance = 8f;

    private Transform _combatTarget;
    private float _combatTargetTimer;

    public Transform Target
    {
        get
        {
            if (_combatTarget != null && _combatTargetTimer > 0f)
            {
                return _combatTarget;
            }

            return _fieldOfView?.CurrentTarget;
        }
    }

    #endregion ===== 타겟 =====

    #region ===== 행동 =====

    private readonly EnemyStateMachine _stateMachine = new();
    private readonly List<EnemyAttackBehaviour> _attackBehaviours = new();

    public EnemyAttackBehaviour CurrentAttackBehaviour { get; private set; }

    public event Action OnAttackFinished;

    public bool IsAttackableDistance
    {
        get
        {
            Transform target = Target;

            if (target == null || CurrentAttackBehaviour == null)
            {
                return false;
            }

            float attackDistance = CurrentAttackBehaviour.AttackableDistance;
            float sqrAttackDistance = attackDistance * attackDistance;

            return (target.position - transform.position).sqrMagnitude <= sqrAttackDistance;
        }
    }

    public bool CanAttack => CurrentAttackBehaviour != null && CurrentAttackBehaviour.IsAvailable &&
                             IsAttackableDistance;

    #endregion ===== 행동 =====

    #region ===== 스탯 =====

    [Header("HP")]
    [SerializeField] private float _maxHp = 100f;

    private float _hp;

    public float MaxHp => _maxHp;
    public float Hp => _hp;

    public event Action<float, float> OnHpChanged;

    [Header("STR")]
    [SerializeField] private float _strength = 10f;

    public float Strength => _strength;

    #endregion ===== 스탯 =====

    #region ===== 넉백 =====

    [Header("넉백")]
    [SerializeField] private float _knockbackDistance = 1f;
    [SerializeField] private float _knockbackDuration = 0.15f;
    [SerializeField] private Ease _knockbackEase = Ease.OutQuad;

    [Header("넉백 충돌 체크")]
    [SerializeField] private LayerMask _knockbackCollisionLayer;
    [SerializeField] private float _knockbackRadius = 0.3f;
    [SerializeField] private float _knockbackCollisionOffset = 0.05f;

    private Tween _knockbackTween;

    #endregion ===== 넉백 =====

    #region ===== 이펙트 =====

    [Header("이펙트")]
    [SerializeField] private ParticleSystem _hitEffect;

    #endregion ===== 이펙트 =====

    #region ===== 디졸브 =====

    [Header("디졸브")]
    [SerializeField] private float _dissolveDuration = 0.5f;
    [SerializeField] private Ease _dissolveEase = Ease.InOutQuad;

    private readonly List<Material> _dissolveMaterials = new();

    private Tween _dissolveTween;
    private float _dissolveValue;

    #endregion ===== 디졸브 =====

    #region ===== 상태 =====

    public bool IsDead { get; protected set; }
    private bool _isPlayerDead;

    public event Action<EnemyCtrl> OnDead;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
        InitializePlayer();
        InitializeSpawnTransform();
        InitializeMaterials();
        InitializeStateMachine();
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void Update()
    {
        if (IsDead)
        {
            return;
        }

        UpdateCombatTarget();
        UpdateCurrentAttackBehaviour();

        _stateMachine.UpdateCurrentState(Time.deltaTime);
    }

    private void OnDestroy()
    {
        KillTweens();
    }

    #region ===== 초기화 =====

    private void InitializeComponents()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _rigid = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _fieldOfView = GetComponent<FieldOfView>();
    }

    private void InitializePlayer()
    {
        if (!Managers.Scene.TryGetCurrentScene(out GameScene gameScene))
        {
            CPrint.Warning("[EnemyCtrl] 현재 GameScene을 찾을 수 없습니다.");
            return;
        }

        _player = gameScene.Player;
    }

    private void InitializeSpawnTransform()
    {
        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;
    }

    private void InitializeStateMachine()
    {
        _stateMachine.RegisterState(new IdleState(_stateMachine, this));
        _stateMachine.RegisterState(new ChaseState(_stateMachine, this));
        _stateMachine.RegisterState(new ReturnState(_stateMachine, this));
        _stateMachine.RegisterState(new HitState(_stateMachine, this));
        _stateMachine.RegisterState(new AttackState(_stateMachine, this));
    }

    private void InitializeMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;

            foreach (Material material in materials)
            {
                if (material != null && material.HasProperty(DissolveProperty))
                {
                    _dissolveMaterials.Add(material);
                }
            }
        }
    }

    public void ResetEnemy()
    {
        KillTweens();

        IsDead = false;
        _isPlayerDead = false;

        ClearCombatTarget();

        _hp = _maxHp;

        _collider.enabled = true;

        ResetRigidbody();
        ResetNavMeshAgent();

        SetDissolveValue(0f);
        SetAnimationMoveSpeed(0f);
    }

    private void ResetRigidbody()
    {
        _rigid.isKinematic = true;
        _rigid.velocity = Vector3.zero;
        _rigid.angularVelocity = Vector3.zero;
    }

    protected void ResetRigidbodyForDeath()
    {
        if (_rigid == null)
        {
            return;
        }

        _rigid.velocity = Vector3.zero;
        _rigid.angularVelocity = Vector3.zero;
        _rigid.isKinematic = true;
    }

    private void ResetNavMeshAgent()
    {
        if (!IsNavMeshAgentAvailable())
        {
            return;
        }

        _navMeshAgent.isStopped = true;
        _navMeshAgent.ResetPath();
        _navMeshAgent.velocity = Vector3.zero;
    }

    protected void ClearCombatTarget()
    {
        _combatTarget = null;
        _combatTargetTimer = 0f;
    }

    #endregion ===== 초기화 =====

    #region ===== 이벤트 =====

    private void SubscribeEvents()
    {
        if (Managers.Event != null)
        {
            Managers.Event.OnPlayerDead += HandlePlayerDead;
        }
    }

    private void UnsubscribeEvents()
    {
        if (Managers.Event != null)
        {
            Managers.Event.OnPlayerDead -= HandlePlayerDead;
        }
    }

    private void HandlePlayerDead()
    {
        if (_isPlayerDead || IsDead)
        {
            return;
        }

        _isPlayerDead = true;
        
        ClearCombatTarget();
        StopMovement();

        _fieldOfView?.StopDetection();
        _fieldOfView?.ClearTarget();
        
        _stateMachine.ChangeState<ReturnState>();
    }

    public void RaiseAttackFinished()
    {
        if (IsDead)
        {
            return;
        }

        OnAttackFinished?.Invoke();
    }

    public void RaiseDead()
    {
        OnDead?.Invoke(this);
    }
    
    #endregion ===== 이벤트 =====

    #region ===== 스폰/디스폰 =====

    public void Spawn(Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            CPrint.Warning("[EnemyCtrl] 스폰 위치가 지정되지 않았습니다.");
            return;
        }

        _spawnPosition = spawnPoint.position;
        _spawnRotation = spawnPoint.rotation;

        transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);

        _navMeshAgent.enabled = true;

        WarpToSpawnPosition();

        ResetEnemy();

        SetDissolveValue(1f);
        PlayDissolve(false);

        _stateMachine.ChangeState<IdleState>();

        OnHpChanged?.Invoke(_hp, _maxHp);
    }

    private void WarpToSpawnPosition()
    {
        if (!_navMeshAgent.isOnNavMesh)
        {
            return;
        }

        _navMeshAgent.Warp(_spawnPosition);
    }

    private void ReturnToPool()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        StopMovement();

        if (!TryGetComponent(out PoolObj poolObj))
        {
            CPrint.Warning($"[EnemyCtrl] {name}에서 PoolObj를 찾을 수 없습니다.");

            return;
        }

        Managers.Pool.Return(poolObj);
    }
    
    #endregion ===== 스폰/디스폰 =====

    #region ===== 타겟 =====

    private void UpdateCombatTarget()
    {
        if (_combatTarget == null)
        {
            return;
        }

        if (IsCombatTargetInRetentionDistance())
        {
            _combatTargetTimer = _combatTargetDuration;
            return;
        }

        _combatTargetTimer -= Time.deltaTime;

        if (_combatTargetTimer > 0f)
        {
            return;
        }

        ClearCombatTarget();
    }

    private bool IsCombatTargetInRetentionDistance()
    {
        if (_combatTarget == null)
        {
            return false;
        }

        float sqrDistance =
            (_combatTarget.position - transform.position).sqrMagnitude;

        float sqrRetentionDistance =
            _combatRetentionDistance * _combatRetentionDistance;

        return sqrDistance <= sqrRetentionDistance;
    }

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

    #region ===== 이동/회전 =====

    public void MoveTo(Vector3 destination, float moveSpeed)
    {
        if (IsDead || !IsNavMeshAgentAvailable())
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

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
            rotationSpeed * deltaTime);
    }

    public void UpdateMovementAnimation()
    {
        if (!IsNavMeshAgentAvailable())
        {
            SetAnimationMoveSpeed(0f);
            return;
        }

        SetAnimationMoveSpeed(_navMeshAgent.velocity.magnitude);
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
        if (!IsNavMeshAgentAvailable())
        {
            SetAnimationMoveSpeed(0f);
            return;
        }

        _navMeshAgent.isStopped = true;
        _navMeshAgent.ResetPath();
        _navMeshAgent.velocity = Vector3.zero;

        SetAnimationMoveSpeed(0f);
    }

    private bool IsNavMeshAgentAvailable()
    {
        return _navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh;
    }

    #endregion ===== 이동/회전 =====

    #region ===== 공격 =====

    public void AddAttackBehaviour(EnemyAttackBehaviour attackBehaviour)
    {
        if (attackBehaviour == null)
        {
            return;
        }

        if (_attackBehaviours.Contains(attackBehaviour))
        {
            return;
        }

        _attackBehaviours.Add(attackBehaviour);
    }

    /// <summary> 현재 타겟과의 거리에 가장 적합한 공격 행동을 선택한다. </summary>
    private void UpdateCurrentAttackBehaviour()
    {
        if (_stateMachine.IsCurrentState<AttackState>())
        {
            return;
        }

        if (_attackBehaviours.Count == 0)
        {
            CurrentAttackBehaviour = null;
            return;
        }

        Transform target = Target;

        if (target == null)
        {
            CurrentAttackBehaviour = null;
            return;
        }

        Vector3 distanceDirection = target.position - transform.position;
        distanceDirection.y = 0f;

        float targetSqrDistance = distanceDirection.sqrMagnitude;

        EnemyAttackBehaviour selectedAttack = null;
        float closestDistanceDifference = float.MaxValue;

        foreach (EnemyAttackBehaviour attackBehaviour in _attackBehaviours)
        {
            if (attackBehaviour == null || !attackBehaviour.IsAvailable)
            {
                continue;
            }

            float sqrAttackableDistance = attackBehaviour.AttackableDistance * attackBehaviour.AttackableDistance;

            float distanceDifference = Mathf.Abs(sqrAttackableDistance - targetSqrDistance);

            if (distanceDifference >= closestDistanceDifference)
            {
                continue;
            }

            closestDistanceDifference = distanceDifference;
            selectedAttack = attackBehaviour;
        }

        CurrentAttackBehaviour = selectedAttack;
    }

    public void ExecuteAttack()
    {
        if (CurrentAttackBehaviour == null)
        {
            return;
        }

        if (Target == null)
        {
            return;
        }

        CurrentAttackBehaviour.ExecuteAttack();
        CurrentAttackBehaviour = null;
    }

    #endregion ===== 공격 =====

    #region ===== 데미지/죽음 =====

    public void TakeDamage(float damage)
    {
        if (IsDead)
        {
            return;
        }

        SetCombatTarget(!_isPlayerDead && _player != null ? _player.transform : null);

        SetHp(-damage);
        PlayHitEffect();
        
        if (IsDead)
        {
            return;
        }

        ApplyKnockbackFromPlayer();
        ChangeToHitState();
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

    private void ChangeToHitState()
    {
        if (_stateMachine.IsCurrentState<HitState>())
        {
            _stateMachine.RestartCurrentState();
            return;
        }

        _stateMachine.ChangeState<HitState>();
    }

    public void OnHitAnimationFinished()
    {
        if (IsDead)
        {
            return;
        }

        if (_stateMachine.TryGetCurrentState(out HitState hitState))
        {
            hitState.SetAnimationFinished();
        }
    }

    private void PlayHitEffect()
    {
        if (_hitEffect == null)
        {
            return;
        }

        if (_player != null &&
            _hitEffect.transform.parent != null)
        {
            _hitEffect.transform.parent.LookAt(_player.transform);
        }

        _hitEffect.Play();
    }

    public virtual void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        RaiseDead();

        KillKnockbackTween();

        StopMovement();
        ClearCombatTarget();

        _fieldOfView?.ClearTarget();

        _collider.enabled = false;

        ResetRigidbodyForDeath();

        _navMeshAgent.enabled = false;

        SetAnimationMoveSpeed(0f);

        if (_animator != null)
        {
            _animator.SetTrigger(AnimatorKey.Hash.DoHit);
        }

        PlayDissolve(true);
    }

    #endregion ===== 데미지/죽음 =====

    #region ===== 넉백 =====

    private void ApplyKnockbackFromPlayer()
    {
        Vector3 attackerPosition = _player != null ? _player.transform.position : transform.position;

        ApplyKnockback(attackerPosition);
    }

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

        StopMovement();
        KillKnockbackTween();

        float knockbackDistance = GetKnockbackDistance(direction);

        if (knockbackDistance <= Mathf.Epsilon)
        {
            return;
        }

        Vector3 targetPosition = transform.position + direction * knockbackDistance;

        _knockbackTween = transform.DOMove(targetPosition, _knockbackDuration).SetEase(_knockbackEase)
            .OnComplete(CompleteKnockback);
    }

    private void CompleteKnockback()
    {
        _knockbackTween = null;

        if (IsDead)
        {
            return;
        }

        if (!IsNavMeshAgentAvailable())
        {
            return;
        }

        _navMeshAgent.Warp(transform.position);
        _navMeshAgent.isStopped = false;
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

    #region ===== 디졸브 =====

    private void SetDissolveValue(float value)
    {
        if (_dissolveMaterials.Count == 0)
        {
            return;
        }
        
        _dissolveValue = value;

        foreach (Material material in _dissolveMaterials)
        {
            material.SetFloat(DissolveProperty, _dissolveValue);
        }
    }

    private void PlayDissolve(bool isDissolving)
    {
        if (_dissolveMaterials.Count == 0)
        {
            return;
        }
        
        float targetValue = isDissolving ? 1f : 0f;

        KillDissolveTween();

        _dissolveTween = DOTween.To(() => _dissolveValue, SetDissolveValue, targetValue, _dissolveDuration)
            .SetEase(_dissolveEase);

        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.EnemyDissolve, 0.5f, true, transform.position);
        
        if (isDissolving)
        {
            _dissolveTween.OnComplete(ReturnToPool);
        }
    }

    #endregion ===== 디졸브 =====

    #region ===== 트윈 =====

    protected void KillTweens()
    {
        KillDissolveTween();
        KillKnockbackTween();
    }

    private void KillDissolveTween()
    {
        _dissolveTween?.Kill();
        _dissolveTween = null;
    }

    private void KillKnockbackTween()
    {
        _knockbackTween?.Kill();
        _knockbackTween = null;
    }

    #endregion ===== 트윈 =====
}