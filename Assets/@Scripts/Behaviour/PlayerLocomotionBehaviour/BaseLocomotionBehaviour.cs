using UnityEngine;

[RequireComponent(typeof(PlayerCtrl))]
public abstract class BaseLocomotionBehaviour : MonoBehaviour
{
    // 1초에 소비되는 SP
    [SerializeField] private float _requiredSpRate;
    public float RequiredSpRate => _requiredSpRate;

    protected PlayerCtrl Player;

    public int BehaviourHash { get; private set; }

    private void Awake()
    {
        Player = GetComponent<PlayerCtrl>();
        Player.AddLocomotionBehaviour(this);
        
        BehaviourHash = GetType().GetHashCode();
    }

    public abstract void OnFixedUpdate();

    private void Update()
    {
        OnUpdateBeforeInput();

        if (!Managers.Input.CanReceiveInput)
        {
            return;
        }

        OnUpdateAfterInput();
    }

    /// <summary> 입력 가능 여부를 확인하기 전에 항상 실행됩니다. </summary>
    protected virtual void OnUpdateBeforeInput()
    {
    }

    /// <summary> 플레이어 입력이 가능한 경우 실행됩니다. </summary>
    protected abstract void OnUpdateAfterInput();
}