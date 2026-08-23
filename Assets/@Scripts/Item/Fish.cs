using DG.Tweening;
using UnityEngine;

public class Fish : MonoBehaviour
{
    [Header("회복")]
    [SerializeField] private float _increaseHp = 30f;

    [Header("획득 연출")]
    [SerializeField] private float _moveDuration = 0.35f;
    [SerializeField] private float _scaleDuration = 0.3f;

    private Collider _collider;
    private ParticleSystem _particle;
    
    private bool _isCollected;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _particle = GetComponentInChildren<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isCollected || !other.CompareTag(TagKey.Player))
        {
            return;
        }

        PlayerCtrl player = other.GetComponent<PlayerCtrl>();

        if (player == null)
        {
            return;
        }

        Collect(player);
    }

    private void Collect(PlayerCtrl player)
    {
        _isCollected = true;

        // 중복 획득 방지
        if (_collider != null)
        {
            _collider.enabled = false;
        }

        // 파티클 종료
        if (_particle != null)
        {
            _particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // HP 회복
        player.SetHp(_increaseHp);

        // 기존 DOTween 연출 제거
        transform.DOKill();
        
        Sequence sequence = DOTween.Sequence();
        // 크기 작아짐
        sequence.Append(transform.DOPunchScale(Vector3.zero, _scaleDuration, 1).SetEase(Ease.InQuad));
        // 위로 치솟음
        sequence.Join(transform.DOMoveY(1, _moveDuration).SetEase(Ease.InBack));

        sequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}