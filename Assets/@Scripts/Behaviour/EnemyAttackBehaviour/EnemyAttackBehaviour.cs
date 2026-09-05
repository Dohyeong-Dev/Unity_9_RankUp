using System;
using UnityEngine;

[RequireComponent(typeof(EnemyCtrl))]
public abstract class EnemyAttackBehaviour : MonoBehaviour
{
    protected EnemyCtrl Enemy;

    [Header("공격 정보")]
    [Tooltip("애니메이터에서 설정된 공격 애니메이션 인덱스")]
    [SerializeField] private int _attackAnimationIndex;
    public int AttackAnimationIndex => _attackAnimationIndex;
    [Tooltip("공격 가능한 거리")]
    [SerializeField] private float _attackableDistance;
    public float AttackableDistance => _attackableDistance;
    [Tooltip("쿨타임")]
    [SerializeField] private float _coolTime;
    private float _coolTimer;
    [Tooltip("데미지 배율"), Range(1f, 10f)]
    [SerializeField] private float _damageFactor = 1f;
    
    public bool IsAvailable => _coolTimer >= _coolTime;
    
    private void Awake()
    {
        Enemy = GetComponent<EnemyCtrl>();
        Enemy.AddAttackBehaviour(this);
        
        _coolTimer = _coolTime;
    }

    private void Update()
    {
        if (!IsAvailable)
        {
            _coolTimer += Time.deltaTime;
        }
    }

    public virtual void ExecuteAttack()
    {
        if (IsAvailable)
        {
            _coolTimer = 0f;
        }
    }
    
    protected float GetRandomDamage()
    {
        float minDamage = Enemy.Strength * 0.8f;
        float maxDamage = Enemy.Strength;

        return UnityEngine.Random.Range(minDamage, maxDamage) * _damageFactor;
    }
}