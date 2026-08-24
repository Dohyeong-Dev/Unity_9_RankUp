using System;
using UnityEngine;

[RequireComponent(typeof(PlayerCtrl))]
public abstract class BaseLocomotionBehaviour : MonoBehaviour
{
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
        if (Player == null)
        {
            return;
        }
        
        OnUpdateAlways();
        
        if (!Player.CanControl)
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

    protected virtual void OnUpdate()
    {
    }
}