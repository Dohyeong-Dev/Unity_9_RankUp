using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EnemyCtrl : MonoBehaviour, IDamageable
{
    [SerializeField] private AttackRange _attackRange;
    [SerializeField] private float _attackDelay = 1;
    [SerializeField] private float _damage = 1;

    private float _hp;
    [SerializeField] private float _maxHp = 30;
    public event Action<float, float> OnHpChanged;

    private bool _isDead;
    public bool IsDead => _isDead;

    private Collider _collider;

    private void Awake()
    {
        _hp = _maxHp;
    }

    private void Start()
    {
        if (_attackRange == null)
        {
            CPrint.Error("No attack range!");
            return;
        }

        _collider = transform.GetComponent<Collider>();
        if (_collider == null)
        {
            CPrint.Warning("No _collider!");
            return;
        }

        StartCoroutine(Co_Attack());
    }

    private IEnumerator Co_Attack()
    {
        WaitForSeconds delay = new WaitForSeconds(_attackDelay == 0 ? 0.1f : _attackDelay);

        while (true)
        {
            Collider[] colliders = _attackRange.GetColliders(LayerKey.Mask.Player);

            for (int i = 0; i < colliders.Length; i++)
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
        if (IsDead)
        {
            return;
        }

        SetHp(-damage);

        if (_hp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (IsDead)
        {
            return;
        }

        _isDead = true;
        _collider.enabled = false;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutFlash)).Join(transform.DOMoveY(
            transform.position.y + 1.5f, 0.3f).SetEase(Ease.OutQuad)).OnComplete(() => { Destroy(gameObject); });
    }

    public void SetHp(float value)
    {
        float previousHp = _hp;

        _hp = Mathf.Clamp(_hp + value, 0f, _maxHp);

        if (!Mathf.Approximately(previousHp, _hp))
        {
            OnHpChanged?.Invoke(_hp, _maxHp);
        }
    }
}