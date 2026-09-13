using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 발사체의 이동, 충돌, 데미지 처리 및 생명 주기를 관리한다. </summary>
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    #region ===== 참조 =====

    private Rigidbody _rigid;
    private Collider _collider;

    #endregion ===== 참조 =====

    #region ===== 소유자 =====

    private GameObject _owner;

    #endregion ===== 소유자 =====

    #region ===== 이동 =====

    private bool _isGuided;
    private Vector3 _moveDirection;
    private float _moveSpeed;
    private Transform _target;
    
    #endregion ===== 이동 =====

    #region ===== 공격 =====

    private float _damage;
    private bool _hasCollided;

    #endregion ===== 공격 =====

    #region ===== 생명 주기 =====

    private float _lifeTime;
    private float _elapsedLifetime;
    private bool _isShrinking;

    [Header("생존 시간 축소")]
    [SerializeField] private bool _useLifetimeShrink = true;

    #endregion ===== 생명 주기 =====

    #region ===== 제거 =====

    [Header("충돌 제거")]
    [SerializeField] private bool _useShrinkOnDestroy = true;
    [SerializeField] private float _shrinkDuration = 0.5f;

    #endregion ===== 제거 =====

    #region ===== 크기 =====

    [Header("최소 크기")]
    [SerializeField]
    [Range(0f, 1f)]
    private float _minimumScaleRatio = 0.1f;

    private List<Transform> _visualTransforms = new();
    private Vector3[] _initialVisualScales;

    #endregion ===== 크기 =====

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        if (_collider != null)
        {
            _collider.enabled = false;
        }

        CacheVisualTransforms();
    }

    private void OnEnable()
    {
        _elapsedLifetime = 0f;
        _hasCollided = false;
        _isShrinking = false;

        ResetSize();
        
        Managers.Event.OnPlayerDead += ReturnPool;

        PlayParticles();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Update()
    {
        UpdateLifetime();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasCollided || IsFriendlyTarget(collision.gameObject))
        {
            return;
        }

        _hasCollided = true;

        StopProjectile();
        ApplyDamage(collision);

        if (!isActiveAndEnabled)
        {
            return;
        }

        ReturnAfterCollision();
    }
    
    private void OnDisable()
    {
        StopAllCoroutines();
        ResetSettings();

        if (Managers.Event != null)
        {
            Managers.Event.OnPlayerDead -= ReturnPool;
        }
    }

    #region ===== 초기화 =====
    
    /// <summary> 발사체와 소유자의 Collider 충돌을 무시하도록 설정한다. </summary>
    private void InitializeOwnerCollision()
    {
        if (_owner == null || _collider == null)
        {
            return;
        }

        Collider[] ownerColliders = _owner.GetComponentsInChildren<Collider>();

        for (int i = 0; i < ownerColliders.Length; i++)
        {
            Physics.IgnoreCollision(_collider, ownerColliders[i]);
        }

        _collider.enabled = true;
    }
    
    /// <summary> 발사체 Visual Transform과 기본 크기를 캐싱한다. </summary>
    private void CacheVisualTransforms()
    {
        if (transform.childCount == 0)
        {
            return;
        }

        Transform[] transforms = transform.GetComponentsInChildren<Transform>();

        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].TryGetComponent<ParticleSystem>(out _))
            {
                _visualTransforms.Add(transforms[i]);
            }
        }
        
        _initialVisualScales = new Vector3[_visualTransforms.Count];

        for (int i = 0; i < _visualTransforms.Count; i++)
        {
            _initialVisualScales[i] = _visualTransforms[i].localScale;
        }
    }

    /// <summary> 발사체를 풀에 반환하기 전 초기 상태로 되돌린다. </summary>
    public void ResetSettings()
    {
        transform.position = Vector3.zero;
        
        _moveDirection = Vector3.zero;
        _moveSpeed = 0f;
        _damage = 0f;
        _lifeTime = 0f;
        _elapsedLifetime = 0f;

        _owner = null;
        _target = null;
        _isGuided = false;

        _hasCollided = false;
        _isShrinking = false;

        ResetSize();

        if (_rigid != null)
        {
            _rigid.isKinematic = false;
        }
    }
    
    /// <summary> 축소된 발사체 Visual의 크기를 기본 크기로 복구한다. </summary>
    private void ResetSize()
    {
        if (_visualTransforms == null || _initialVisualScales == null)
        {
            return;
        }

        for (int i = 0; i < _visualTransforms.Count; i++)
        {
            _visualTransforms[i].localScale = _initialVisualScales[i];
        }
    }

    private void PlayParticles()
    {
        for (int i = 0; i < _visualTransforms.Count; i++)
        {
            _visualTransforms[i].GetComponent<ParticleSystem>().Play();
        }
    }
    
    #endregion ===== 초기화 =====

    #region ===== 이동 =====

    /// <summary> 설정에 따라 직선 또는 유도 방식으로 발사체를 이동시킨다. </summary>
    private void Move()
    {
        if (_moveSpeed <= Mathf.Epsilon || _isShrinking)
        {
            return;
        }

        if (_isGuided && _target != null)
        {
            MoveGuided();
            return;
        }

        MoveStraight();
    }

    /// <summary> 설정된 방향으로 발사체를 직선 이동시킨다. </summary>
    private void MoveStraight()
    {
        transform.position += _moveDirection * _moveSpeed * Time.fixedDeltaTime;
    }

    /// <summary> 발사된 높이를 유지하면서 목표의 수평 위치를 추적한다. </summary>
    private void MoveGuided()
    {
        Vector3 direction = _target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        _moveDirection = direction.normalized;

        transform.rotation = Quaternion.LookRotation(_moveDirection);
        transform.position += _moveDirection * _moveSpeed * Time.fixedDeltaTime;
    }

    #endregion ===== 이동 =====

    #region ===== 생명 주기 =====

    /// <summary> 발사체의 생존 시간을 갱신하고 필요하면 크기를 축소한다. </summary>
    private void UpdateLifetime()
    {
        if (_isShrinking)
        {
            return;
        }

        _elapsedLifetime += Time.deltaTime;

        UpdateLifetimeShrink();

        if (_elapsedLifetime < _lifeTime)
        {
            return;
        }

        ReturnPool();
    }

    /// <summary> 생존 시간 진행도에 따라 발사체의 크기를 축소한다. </summary>
    private void UpdateLifetimeShrink()
    {
        if (!_useLifetimeShrink || _lifeTime <= Mathf.Epsilon)
        {
            return;
        }

        float progress = Mathf.Clamp01(_elapsedLifetime / _lifeTime);
        SetVisualScale(progress);
    }
    
    #endregion ===== 생명 주기 =====

    #region ===== 충돌 =====

    /// <summary> 충돌한 대상이 소유자와 같은 레이어인지 확인한다. </summary>
    private bool IsFriendlyTarget(GameObject target)
    {
        return _owner != null && _owner.layer == target.layer;
    }

    /// <summary> 충돌한 대상에게 데미지와 필요한 피격 효과를 적용한다. </summary>
    private void ApplyDamage(Collision collision)
    {
        GameObject target = collision.gameObject;

        if (IsFriendlyTarget(target))
        {
            return;
        }

        if (!target.TryGetComponent(out PlayerCtrl player))
        {
            return;
        }

        ContactPoint contactPoint = collision.GetContact(0);
        player.PlayHitEffect(contactPoint.point + contactPoint.normal * 0.4f, contactPoint.normal);
        
        if (!target.TryGetComponent(out IDamageable damageable))
        {
            return;
        }

        damageable.TakeDamage(_damage);
    }

    /// <summary> 충돌 후 발사체의 이동과 물리 동작을 중지한다. </summary>
    private void StopProjectile()
    {
        _moveSpeed = 0f;

        if (_rigid != null)
        {
            _rigid.isKinematic = true;
        }
    }

    /// <summary> 충돌 후 설정에 따라 즉시 반환하거나 축소 효과를 재생한다. </summary>
    private void ReturnAfterCollision()
    {
        if (!_useShrinkOnDestroy || _shrinkDuration <= Mathf.Epsilon)
        {
            ReturnPool();
            return;
        }

        _isShrinking = true;
        StartCoroutine(ShrinkAndRemove());
    }

    #endregion ===== 충돌 =====

    #region ===== 제거 =====

    /// <summary> 현재 크기에서 지정된 시간 동안 축소한 후 풀에 반환한다. </summary>
    private IEnumerator ShrinkAndRemove()
    {
        if (_visualTransforms == null)
        {
            ReturnPool();
            yield break;
        }

        Vector3[] startScales = new Vector3[_visualTransforms.Count];

        for (int i = 0; i < _visualTransforms.Count; i++)
        {
            startScales[i] = _visualTransforms[i].localScale;
        }

        float elapsedTime = 0f;

        while (elapsedTime < _shrinkDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsedTime / _shrinkDuration);

            SetVisualScale(progress);

            yield return null;
        }

        ReturnPool();
    }

    /// <summary> 발사체를 오브젝트 풀에 반환한다. </summary>
    private void ReturnPool()
    {
        if (!TryGetComponent(out PoolObj poolObj))
        {
            CPrint.Warning($"[Projectile] {name}에서 PoolObj를 찾을 수 없습니다.");

            return;
        }
        Managers.Pool.Return(poolObj);
    }

    #endregion ===== 제거 =====

    #region ===== 설정 =====

    /// <summary> 진행도에 따라 설정된 최소 크기까지 발사체 Visual의 크기를 조절한다. </summary>
    private void SetVisualScale(float progress)
    {
        if (_visualTransforms == null || _initialVisualScales == null)
        {
            return;
        }

        for (int i = 0; i < _visualTransforms.Count; i++)
        {
            Vector3 minimumScale = _initialVisualScales[i] * _minimumScaleRatio;

            _visualTransforms[i].localScale = Vector3.Lerp(_initialVisualScales[i], minimumScale, progress);
        }
    }
    
    /// <summary> 발사체의 위치와 이동 및 공격 정보를 설정한다. </summary>
    public void SetProjectile(GameObject owner, Vector3 spawnPosition, Vector3 direction, float speed, float damage,
        float lifeTime, bool isGuided, Transform target = null)
    {
        _owner = owner;
        _target = target;
        _isGuided = isGuided;

        transform.position = spawnPosition;
    
        _moveDirection = direction.normalized;
        _moveSpeed = speed;
        _damage = damage;
        _lifeTime = lifeTime;

        _elapsedLifetime = 0f;
        _hasCollided = false;
        _isShrinking = false;

        ResetSize();
        InitializeOwnerCollision();
    }

    #endregion ===== 설정 =====
}