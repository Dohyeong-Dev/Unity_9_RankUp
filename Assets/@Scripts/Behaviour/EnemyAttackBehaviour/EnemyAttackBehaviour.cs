using UnityEngine;

/// <summary> 적의 공격 행동에 필요한 공통 정보와 쿨타임을 관리하는 추상 클래스다. </summary>
[RequireComponent(typeof(EnemyCtrl))]
public abstract class EnemyAttackBehaviour : MonoBehaviour
{
    #region ===== 컴포넌트 =====

    protected EnemyCtrl Enemy;

    #endregion ===== 컴포넌트 =====

    #region ===== 공격 정보 =====

    [Header("공격 정보")]
    
    [Tooltip("애니메이터에서 설정된 공격 애니메이션 인덱스")]
    [SerializeField] private int _attackAnimationIndex;
    public int AttackAnimationIndex => _attackAnimationIndex;

    [Tooltip("공격 가능한 거리")]
    [SerializeField] private float _attackableDistance;
    public float AttackableDistance => _attackableDistance;

    [Tooltip("데미지 배율"), Range(1f, 10f)]
    [SerializeField] private float _damageFactor = 1f;

    #endregion ===== 공격 정보 =====

    #region ===== 쿨타임 =====

    [Header("쿨타임")]
    [SerializeField] private float _coolTime;

    private float _coolTimer;

    /// <summary> 현재 공격 행동을 사용할 수 있는지 반환한다. </summary>
    public bool IsAvailable => _coolTimer <= 0f;

    #endregion ===== 쿨타임 =====

    private void Start()
    {
        InitializeEnemy();
        InitializeCooldown();
    }

    private void Update()
    {
        UpdateCooldown();
    }

    #region ===== 초기화 =====

    /// <summary> 적 컨트롤러를 초기화하고 공격 행동으로 등록한다. </summary>
    private void InitializeEnemy()
    {
        Enemy = GetComponent<EnemyCtrl>();
        Enemy.AddAttackBehaviour(this);
    }

    /// <summary> 시작 시 공격 행동을 즉시 사용할 수 있도록 쿨타임을 초기화한다. </summary>
    private void InitializeCooldown()
    {
        _coolTimer = 0f;
    }

    #endregion ===== 초기화 =====

    #region ===== 쿨타임 =====

    /// <summary> 남은 공격 쿨타임을 갱신한다. </summary>
    private void UpdateCooldown()
    {
        if (IsAvailable)
        {
            return;
        }

        _coolTimer -= Time.deltaTime;
        _coolTimer = Mathf.Max(_coolTimer, 0f);
    }

    /// <summary> 공격 사용 후 쿨타임을 시작한다. </summary>
    private void StartCooldown()
    {
        _coolTimer = _coolTime;
    }

    #endregion ===== 쿨타임 =====

    #region ===== 공격 =====

    /// <summary> 현재 공격 행동을 실행한다. </summary>
    public virtual void ExecuteAttack()
    {
        if (!IsAvailable)
        {
            return;
        }

        StartCooldown();
    }

    /// <summary> 적의 공격력과 데미지 배율을 기준으로 랜덤 데미지를 계산한다. </summary>
    protected int GetRandomDamage()
    {
        float minDamage = Enemy.Strength * 0.5f;
        float maxDamage = Enemy.Strength * 1.5f;

        return Mathf.RoundToInt(Random.Range(minDamage, maxDamage) * _damageFactor);
    }

    #endregion ===== 공격 =====
}