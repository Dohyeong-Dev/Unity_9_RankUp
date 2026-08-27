using System;
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
        
        BehaviourHash = GetType().GetHashCode();
    }

    public abstract void OnFixedUpdate();

    private void Update()
    {
        OnUpdateAlways();
        
        if (!Managers.Input.CanReceiveInput)
        {
            return;
        }
        
        OnUpdate();
    }
    
    /// <summary> UI 입력 상태나 현재 행동과 관계없이 항상 실행됩니다. </summary>
    protected virtual void OnUpdateAlways()
    {
    }

    /// <summary> 현재 행동이 활성 상태이고 플레이어 입력이 가능한 경우 실행됩니다. </summary>
    protected abstract void OnUpdate();
}