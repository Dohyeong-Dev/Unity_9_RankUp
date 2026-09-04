using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PlayerCtrl))]
public abstract class PlayerAttackBehaviour : MonoBehaviour
{
    protected PlayerCtrl Player;
    
    [Header("공격 정보")]
    // 공격 한번 당 소비되는 SP
    [SerializeField] private float _requiredSp;
    public float RequiredSp => _requiredSp;
    // 데미지 배율
    [Range(1f, 10f)]
    [SerializeField] private float _damageFactor = 1f;
    
    private void Awake()
    {
        Player = GetComponent<PlayerCtrl>();
    }

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    protected abstract void OnFixedUpdate();
    
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

    /// <summary> 현재 행동이 활성 상태이고 플레이어 입력이 가능한 경우 실행됩니다. </summary>
    protected abstract void OnUpdate();

    public abstract void Clear();
    
    protected float GetRandomDamage()
    {
        float minDamage = Player.STR * 0.8f;
        float maxDamage = Player.STR;

        return Random.Range(minDamage, maxDamage) * _damageFactor;
    }
}
