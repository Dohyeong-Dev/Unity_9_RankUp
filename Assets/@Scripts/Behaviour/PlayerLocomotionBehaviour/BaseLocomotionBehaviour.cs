using UnityEngine;

/// <summary> 플레이어의 이동 관련 Behaviour가 공통으로 사용하는 기본 기능과 입력 처리 흐름을 제공한다. </summary>
[RequireComponent(typeof(PlayerCtrl))]
public abstract class BaseLocomotionBehaviour : MonoBehaviour
{
    public int BehaviourHash { get; private set; }
    
    #region ===== 참조 =====

    protected PlayerCtrl Player;

    #endregion ===== 참조 =====

    #region ===== 설정 =====

    // 1초당 소비되는 SP
    [SerializeField] private float _requiredSpRate;
    public float RequiredSpRate => _requiredSpRate;

    #endregion ===== 설정 =====

    private void Awake()
    {
        Player = GetComponent<PlayerCtrl>();
        Player.AddLocomotionBehaviour(this);

        BehaviourHash = GetType().GetHashCode();
    }

    private void Update()
    {
        OnUpdateBeforeInput();

        if (!Managers.Input.CanReceivePlayer || Player.IsDead)
        {
            return;
        }
        
        OnUpdateAfterInput();
    }

    /// <summary> PlayerCtrl의 FixedUpdate에서 각 이동 Behaviour의 물리 처리를 수행한다. </summary>
    public abstract void OnFixedUpdate();

    /// <summary> 플레이어 입력 가능 여부를 확인하기 전에 항상 실행한다. </summary>
    protected virtual void OnUpdateBeforeInput()
    {
    }

    /// <summary> 플레이어 입력이 가능한 경우 입력 관련 처리를 수행한다. </summary>
    protected abstract void OnUpdateAfterInput();
}