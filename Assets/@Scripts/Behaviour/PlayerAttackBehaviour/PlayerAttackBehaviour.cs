using UnityEngine;

/// <summary> 플레이어 공격 Behaviour의 공통 기능과 실행 흐름을 관리하는 추상 클래스다. </summary>
[RequireComponent(typeof(PlayerCtrl))]
public abstract class PlayerAttackBehaviour : MonoBehaviour
{
    #region ===== 참조 =====

    protected PlayerCtrl Player;

    #endregion ===== 참조 =====

    #region ===== 설정 =====

    [Header("공격 정보")]

    [Tooltip("공격 1회당 소비되는 SP")]
    [SerializeField] private float _requiredSp;
    public float RequiredSp => _requiredSp;

    [Tooltip("최종 데미지에 적용되는 배율"), Range(1f, 10f)]
    [SerializeField] private float _damageFactor = 1f;

    #endregion ===== 설정 =====


    #region ===== 초기화 =====

    private void Awake()
    {
        Player = GetComponent<PlayerCtrl>();
    }

    #endregion ===== 초기화 =====

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    private void Update()
    {
        if (!Managers.Input.CanReceiveInput)
        {
            return;
        }

        if (Player.IsDead)
        {
            return;
        }

        OnUpdate();
    }

    #region ===== 업데이트 =====

    /// <summary> 물리 업데이트가 필요한 공격 Behaviour의 처리를 수행한다. </summary>
    protected abstract void OnFixedUpdate();

    /// <summary> 플레이어 입력이 가능한 상태에서 공격 Behaviour의 업데이트를 수행한다. </summary>
    protected abstract void OnUpdate();

    #endregion ===== 업데이트 =====

    #region ===== 공격 =====

    /// <summary> 현재 공격 Behaviour의 상태를 초기화한다. </summary>
    public abstract void Clear();

    /// <summary> 플레이어의 공격력과 데미지 배율을 기반으로 랜덤 데미지를 계산한다. </summary>
    protected int GetRandomDamage()
    {
        float minDamage = Player.STR * 0.5f;
        float maxDamage = Player.STR * 1.5f;

        return Mathf.RoundToInt(Random.Range(minDamage, maxDamage) * _damageFactor);
    }

    #endregion ===== 공격 =====
}