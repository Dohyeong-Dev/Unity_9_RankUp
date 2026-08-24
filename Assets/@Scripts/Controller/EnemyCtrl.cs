using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCtrl : MonoBehaviour, IDamageable
{
    [SerializeField] private AttackRange _attackRange;
    [SerializeField] private float _attackDelay = 1;
    [SerializeField] private float _damage = 1;

    [SerializeField] private float _hp;
    public bool IsDead => _hp <= 0;
    
    private void Start()
    {
        if (_attackRange == null)
        {
            CPrint.Error("No attack range!");
        }
        
        StartCoroutine(Co_Attack());
    }

    private IEnumerator Co_Attack()
    {
        WaitForSeconds delay = new WaitForSeconds(_attackDelay == 0 ? 0.1f : _attackDelay);
        
        while (true)
        {
            Collider[] colliders = _attackRange.GetColliders(LayerKey.Mask.Player);

            for (int i = 0; i <  colliders.Length; i++)
            {
                if (colliders[i].TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    damageable.TakeDamage(_damage);
                }
            }
            yield return delay;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
    
    public void TakeDamage(float damage)
    {
    }

    public void Die()
    {
    }
}
