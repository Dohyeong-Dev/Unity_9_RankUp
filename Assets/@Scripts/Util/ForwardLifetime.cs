using DG.Tweening;
using UnityEngine;

/// <summary> 오브젝트를 설정된 방향으로 이동시키고 일정 시간이 지나면 비활성화하거나 파괴한다. </summary>
public class ForwardLifetime : MonoBehaviour
{
    #region ===== 세팅 =====

    [Header("설정")]
    [Tooltip("이동 속도")]
    [SerializeField] private float _moveSpeed = 10f;

    [Tooltip("생명 주기")]
    [SerializeField] private float _lifeTime = 3f;

    [Tooltip("생명 주기를 도달하면 파괴")]
    [SerializeField] private bool _destroyOnLifetimeEnd;

    #endregion ===== 세팅 =====

    #region ===== 이동 =====

    [Header("이동 연출")]
    [Tooltip("이동 Ease")]
    [SerializeField] private Ease _moveEase = Ease.Linear;

    private Tween _moveTween;

    #endregion ===== 이동 =====

    private void OnEnable()
    {
        StartMovement();
    }

    private void OnDisable()
    {
        _moveTween?.Kill();
        _moveTween = null;
    }

    /// <summary> 설정된 방향과 거리만큼 DOTween을 사용하여 이동한다. </summary>
    private void StartMovement()
    {
        if (_lifeTime <= Mathf.Epsilon)
        {
            RemoveObject();
            return;
        }

        Vector3 movePosition = transform.position + transform.forward * _moveSpeed * _lifeTime;

        _moveTween = transform.DOMove(movePosition, _lifeTime).SetEase(_moveEase).OnComplete(RemoveObject);
    }

    /// <summary> 설정에 따라 오브젝트를 파괴하거나 비활성화한다. </summary>
    private void RemoveObject()
    {
        if (_destroyOnLifetimeEnd)
        {
            Destroy(gameObject);
            return;
        }

        gameObject.SetActive(false);
    }
}