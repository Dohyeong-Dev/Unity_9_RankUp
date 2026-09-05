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
    #region ===== 컴포넌트 =====

    private PlayerCtrl _player;

    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private Rigidbody _rigid;
    private Collider _collider;
    private FieldOfView _fieldOfView;

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

    /// <summary> 현재 공격 행동의 공격 가능 거리를 우선 사용하고 없으면 기본 추적 정지 거리를 반환한다. </summary>
    public float ChaseStoppingDistance => CurrentAttackBehaviour == null
        ? _defaultChaseStoppingDistance
        : CurrentAttackBehaviour.AttackableDistance;

    #endregion ===== 이동 =====

    #region ===== 타겟 =====

    private Transform _combatTarget;
    private float _combatTargetTimer;

    [Header("전투 타겟")]
    [Tooltip("적이 플레이어를 마지막으로 인식한 후 전투 타겟을 유지하는 시간")]
    [SerializeField] private float _combatTargetDuration = 3f;
    [Tooltip("전투 타겟이 이 거리 안에 있으면 전투 타겟 유지 시간이 초기화되는 거리")]
    [SerializeField] private float _combatRetentionDistance = 8f;

    /// <summary> 유지 중인 전투 타겟을 우선 반환하고 없으면 시야각 시스템의 현재 타겟을 반환한다. </summary>
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

    #region ===== 행동 =====

    private readonly EnemyStateMachine _stateMachine = new();
    private readonly List<EnemyAttackBehaviour> _attackBehaviours = new();

    public EnemyAttackBehaviour CurrentAttackBehaviour { get; private set; }

    public event Action OnAttackFinished;

    /// <summary> 현재 타겟이 현재 공격 행동의 공격 가능 거리 안에 있는지 반환한다. </summary>
    public bool IsAttackableDistance
    {
        get
        {
            if (Target == null || CurrentAttackBehaviour == null)
            {
                return false;
            }

            float sqrDistance = (Target.position - transform.position).sqrMagnitude;
            float attackDistance = CurrentAttackBehaviour.AttackableDistance;
            float sqrAttackDistance = attackDistance * attackDistance;

            return sqrDistance <= sqrAttackDistance;
        }
    }

    #endregion ===== 행동 =====

    #region ===== 스탯 =====

    [Header("HP")]
    [SerializeField] private float _maxHp = 100f;
    public float MaxHp => _maxHp;

    private float _hp;
    public float Hp => _hp;

    public event Action<float, float> OnHpChanged;

    [Header("STR")]
    [SerializeField] private float _strength = 10f;
    public float Strength => _strength;

    #endregion ===== 스탯 =====

    #region ===== 넉백 =====

    private Tween _knockbackTween;

    [Header("넉백")]
    [SerializeField] private float _knockbackDistance = 1f;
    [SerializeField] private float _knockbackDuration = 0.15f;
    [SerializeField] private Ease _knockbackEase = Ease.OutQuad;

    [Header("넉백 충돌 체크")]
    [SerializeField] private LayerMask _knockbackCollisionLayer;
    [SerializeField] private float _knockbackRadius = 0.3f;
    [SerializeField] private float _knockbackCollisionOffset = 0.05f;

    #endregion ===== 넉백 =====

    #region ===== 이펙트 =====

    [Header("이펙트")]
    [SerializeField] private ParticleSystem _hitEffect;

    #endregion ===== 이펙트 =====

    #region ===== 디졸브 =====

    private Tween _dissolveTween;
    private readonly List<Material> _dissolveMaterials = new();
    private float _dissolveValue;

    [Header("디졸브")]
    [SerializeField] private float _dissolveDuration = 0.5f;
    [SerializeField] private Ease _dissolveEase = Ease.InOutQuad;

    #endregion ===== 디졸브 =====

    #region ===== 상태 =====

    public bool IsDead { get; private set; }

    public event Action<EnemyCtrl> OnDead;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
        InitializePlayer();
        InitializeSpawnTransform();
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
        UpdateCurrentAttackBehaviour();
    }

    private void OnEnable()
    {
        if (Managers.Event == null)
        {
            return;
        }

        Managers.Event.OnPlayerDead += HandlePlayerDead;
    }

    private void OnDisable()
    {
        if (Managers.Event == null)
        {
            return;
        }

        Managers.Event.OnPlayerDead -= HandlePlayerDead;
    }

    private void OnDestroy()
    {
        KillTweens();
    }

    #region ===== 초기화 =====

    /// <summary> 적이 사용하는 필수 컴포넌트를 초기화한다. </summary>
    private void InitializeComponents()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _rigid = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _fieldOfView = GetComponent<FieldOfView>();
    }

    /// <summary> 현재 게임 씬에서 플레이어 참조를 가져온다. </summary>
    private void InitializePlayer()
    {
        if (!Managers.Scene.TryGetCurrentScene(out GameScene gameScene))
        {
            CPrint.Warning("현재 GameScene을 찾을 수 없습니다.");
            return;
        }

        _player = gameScene.Player;
    }

    /// <summary> 적의 초기 위치와 회전을 스폰 위치로 저장한다. </summary>
    private void InitializeSpawnTransform()
    {
        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;
    }

    /// <summary> 적의 상태 머신에 사용할 모든 상태를 등록한다. </summary>
    private void InitializeStateMachine()
    {
        _stateMachine.RegisterState(new IdleState(_stateMachine, this));
        _stateMachine.RegisterState(new ChaseState(_stateMachine, this));
        _stateMachine.RegisterState(new ReturnState(_stateMachine, this));
        _stateMachine.RegisterState(new HitState(_stateMachine, this));
        _stateMachine.RegisterState(new AttackState(_stateMachine, this));
    }

    /// <summary> 디졸브 속성을 사용하는 모든 렌더러의 마테리얼을 수집한다. </summary>
    private void InitializeMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;

            foreach (Material material in materials)
            {
                if (material != null && material.HasProperty("_Dissolve"))
                {
                    _dissolveMaterials.Add(material);
                }
            }
        }
    }
    
    /// <summary> 적의 모든 상태를 초기값으로 되돌린다. </summary>
    public void ResetEnemy()
    {
        KillTweens();
        ClearCombatTarget();

        _hp = _maxHp;
        IsDead = false;

        if (_collider != null)
        {
            _collider.enabled = true;
        }

        ResetRigidbody();
        ResetNavMeshAgent();

        SetDissolveValue(0f);
        SetAnimationMoveSpeed(0f);
    }

    /// <summary> Rigidbody의 물리 상태와 속도를 초기화한다. </summary>
    private void ResetRigidbody()
    {
        if (_rigid == null)
        {
            return;
        }

        _rigid.isKinematic = true;
        _rigid.velocity = Vector3.zero;
        _rigid.angularVelocity = Vector3.zero;
    }

    /// <summary> NavMeshAgent의 이동 상태와 경로를 초기화한다. </summary>
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

    #endregion ===== 초기화 =====

    #region ===== 스폰 =====

    /// <summary> 지정된 위치에서 적을 초기화하고 스폰한다. </summary>
    public void Spawn(Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            CPrint.Warning("스폰 위치가 지정되지 않았습니다.");
            return;
        }

        _spawnPosition = spawnPoint.position;
        _spawnRotation = spawnPoint.rotation;

        transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);

        EnableNavMeshAgent();

        if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.Warp(_spawnPosition);
        }

        ResetEnemy();

        SetDissolveValue(1f);
        PlayDissolve(false);

        _stateMachine.ChangeState<IdleState>();

        OnHpChanged?.Invoke(_hp, _maxHp);
    }

    /// <summary> 비활성화된 NavMeshAgent를 활성화한다. </summary>
    private void EnableNavMeshAgent()
    {
        if (_navMeshAgent == null || _navMeshAgent.enabled)
        {
            return;
        }

        _navMeshAgent.enabled = true;
    }

    #endregion ===== 스폰 =====

    #region ===== 이벤트 =====

    /// <summary> 플레이어 사망 시 전투를 중단하고 복귀 상태로 전환한다. </summary>
    private void HandlePlayerDead()
    {
        if (IsDead)
        {
            return;
        }

        ClearCombatTarget();
        _fieldOfView?.ClearTarget();

        StopMovement();

        _stateMachine.ChangeState<ReturnState>();
    }

    /// <summary> 공격 애니메이션이 종료되었음을 외부에 알린다. </summary>
    public void RaiseAttackFinished()
    {
        if (IsDead)
        {
            return;
        }

        OnAttackFinished?.Invoke();
    }
    
    #endregion ===== 이벤트 =====

    #region ===== 타겟 =====

    /// <summary> 전투 타겟의 거리와 유지 시간을 갱신한다. </summary>
    private void UpdateCombatTarget()
    {
        if (_combatTarget == null)
        {
            return;
        }

        float sqrDistance = (_combatTarget.position - transform.position).sqrMagnitude;
        float sqrRetentionDistance = _combatRetentionDistance * _combatRetentionDistance;

        if (sqrDistance <= sqrRetentionDistance)
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

    /// <summary> 지정된 대상을 일정 시간 동안 전투 타겟으로 유지한다. </summary>
    private void SetCombatTarget(Transform target)
    {
        if (target == null)
        {
            return;
        }

        _combatTarget = target;
        _combatTargetTimer = _combatTargetDuration;
    }

    /// <summary> 현재 유지 중인 전투 타겟 정보를 초기화한다. </summary>
    private void ClearCombatTarget()
    {
        _combatTarget = null;
        _combatTargetTimer = 0f;
    }

    #endregion ===== 타겟 =====

    #region ===== 이동 및 회전 =====

    /// <summary> 지정된 목적지까지 설정된 속도로 이동한다. </summary>
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

    /// <summary> 지정된 위치를 바라보도록 설정된 속도로 회전한다. </summary>
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

    /// <summary> 현재 NavMeshAgent의 실제 이동 속도를 애니메이션에 반영한다. </summary>
    public void UpdateMovementAnimation()
    {
        if (!IsNavMeshAgentAvailable())
        {
            SetAnimationMoveSpeed(0f);
            return;
        }

        SetAnimationMoveSpeed(_navMeshAgent.velocity.magnitude);
    }

    /// <summary> 이동 속도를 애니메이터에서 사용하는 속도 값으로 변환하여 적용한다. </summary>
    public void SetAnimationMoveSpeed(float moveSpeed)
    {
        if (_animator == null)
        {
            return;
        }

        _animator.SetFloat(AnimatorKey.Hash.Speed, moveSpeed * _animationSpeedMultiplier);
    }

    /// <summary> 현재 이동을 중지하고 NavMeshAgent의 경로를 초기화한다. </summary>
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

    /// <summary> NavMeshAgent가 현재 이동 가능한 상태인지 반환한다. </summary>
    private bool IsNavMeshAgentAvailable()
    {
        return _navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh;
    }

    #endregion ===== 이동 및 회전 =====

    #region ===== 공격 =====

    /// <summary> 적이 사용할 공격 행동을 등록한다. </summary>
    public void AddAttackBehaviour(EnemyAttackBehaviour attackBehaviour)
    {
        if (attackBehaviour == null || _attackBehaviours.Contains(attackBehaviour))
        {
            return;
        }

        _attackBehaviours.Add(attackBehaviour);
    }

    /// <summary> 현재 공격 행동이 없거나 사용할 수 없으면 사용 가능한 공격 행동 중 하나를 선택한다. </summary>
    private void UpdateCurrentAttackBehaviour()
    {
        if (CurrentAttackBehaviour != null && CurrentAttackBehaviour.IsAvailable)
        {
            return;
        }

        CurrentAttackBehaviour = null;

        int attackCount = _attackBehaviours.Count;

        if (attackCount == 0)
        {
            return;
        }

        int startIndex = UnityEngine.Random.Range(0, attackCount);

        for (int i = 0; i < attackCount; i++)
        {
            int attackIndex = (startIndex + i) % attackCount;
            EnemyAttackBehaviour attackBehaviour = _attackBehaviours[attackIndex];

            if (attackBehaviour == null || !attackBehaviour.IsAvailable)
            {
                continue;
            }

            CurrentAttackBehaviour = attackBehaviour;
            return;
        }
    }

    /// <summary> 현재 선택된 공격 행동을 실행하고 공격 행동 참조를 초기화한다. </summary>
    public void ExecuteAttack()
    {
        if (CurrentAttackBehaviour == null || Target == null)
        {
            return;
        }

        CurrentAttackBehaviour.ExecuteAttack();
        CurrentAttackBehaviour = null;
    }

    #endregion ===== 공격 =====

    #region ===== 데미지 =====

    /// <summary> 피해를 적용하고 생존 상태라면 피격 반응과 넉백을 처리한다. </summary>
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

        Vector3 attackerPosition = _player != null ? _player.transform.position : transform.position;

        ApplyKnockback(attackerPosition);

        if (_stateMachine.IsCurrentState<HitState>())
        {
            _stateMachine.RestartCurrentState();
            return;
        }

        _stateMachine.ChangeState<HitState>();
    }

    /// <summary> HP 값을 변경하고 값이 변경되었을 때 UI 갱신 이벤트를 호출한다. </summary>
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

    /// <summary> 피격 애니메이션 종료 이벤트를 현재 HitState에 전달한다. </summary>
    public void OnHitAnimationFinished()
    {
        if (IsDead)
        {
            return;
        }

        if (_stateMachine.TryGetCurrentState<HitState>(out HitState hitState))
        {
            hitState.SetAnimationFinished();
        }
    }

    /// <summary> 피격 이펙트를 재생한다. </summary>
    private void PlayHitEffect()
    {
        if (_hitEffect == null)
        {
            return;
        }

        if (_player != null && _hitEffect.transform.parent != null)
        {
            _hitEffect.transform.parent.LookAt(_player.transform);
        }

        _hitEffect.Play();
    }

    #endregion ===== 데미지 =====

    #region ===== 죽음 =====

    /// <summary> 적의 모든 행동을 중단하고 죽음 연출을 시작한다. </summary>
    public void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        OnDead?.Invoke(this);

        KillKnockbackTween();

        StopMovement();
        ClearCombatTarget();

        DisableCollider();
        ResetDeathPhysics();
        DisableNavMeshAgent();

        SetAnimationMoveSpeed(0f);

        if (_animator != null)
        {
            _animator.SetTrigger(AnimatorKey.Hash.DoHit);
        }

        PlayDissolve(true);
    }

    /// <summary> 죽은 적의 물리 이동을 중지한다. </summary>
    private void ResetDeathPhysics()
    {
        if (_rigid == null)
        {
            return;
        }

        _rigid.velocity = Vector3.zero;
        _rigid.angularVelocity = Vector3.zero;
        _rigid.isKinematic = true;
    }

    /// <summary> 적의 충돌체를 비활성화한다. </summary>
    private void DisableCollider()
    {
        if (_collider != null)
        {
            _collider.enabled = false;
        }
    }

    /// <summary> 활성화된 NavMeshAgent를 비활성화한다. </summary>
    private void DisableNavMeshAgent()
    {
        if (_navMeshAgent != null && _navMeshAgent.enabled)
        {
            _navMeshAgent.enabled = false;
        }
    }

    #endregion ===== 죽음 =====

    #region ===== 넉백 =====

    /// <summary> 공격자 반대 방향으로 적을 이동시키고 NavMesh 위치를 동기화한다. </summary>
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

    /// <summary> 넉백이 종료된 후 NavMeshAgent의 위치와 이동 상태를 동기화한다. </summary>
    private void CompleteKnockback()
    {
        _knockbackTween = null;

        if (IsDead)
        {
            return;
        }

        if (IsNavMeshAgentAvailable())
        {
            _navMeshAgent.Warp(transform.position);
            _navMeshAgent.isStopped = false;
        }
    }

    /// <summary> 충돌 가능한 장애물을 검사하여 실제 적용할 넉백 거리를 계산한다. </summary>
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

    #region ===== 디졸브 및 풀링 =====

    /// <summary> 모든 디졸브 마테리얼에 현재 디졸브 값을 적용한다. </summary>
    private void SetDissolveValue(float value)
    {
        _dissolveValue = value;

        foreach (Material material in _dissolveMaterials)
        {
            material.SetFloat("_Dissolve", value);
        }
    }

    /// <summary> 디졸브 방향에 따라 등장 또는 사라지는 연출을 재생한다. </summary>
    private void PlayDissolve(bool isDissolving)
    {
        float targetValue = isDissolving ? 1f : 0f;

        KillDissolveTween();
        _dissolveTween = DOTween.To(() => _dissolveValue, SetDissolveValue, targetValue, _dissolveDuration)
            .SetEase(_dissolveEase);

        if (isDissolving)
        {
            _dissolveTween.OnComplete(ReturnToPool);
        }
    }

    /// <summary> 적의 모든 연출이 종료된 후 오브젝트 풀로 반환한다. </summary>
    private void ReturnToPool()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        StopMovement();

        if (!TryGetComponent(out PoolObj poolObj))
        {
            CPrint.Warning($"{name}에서 PoolObj를 찾을 수 없습니다.");
            return;
        }

        Managers.Pool.Return(poolObj);
    }

    #endregion ===== 디졸브 및 풀링 =====
    
    #region ===== 트윈 =====
    
    /// <summary> 실행 중인 DOTween을 모두 종료한다. </summary>
    private void KillTweens()
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