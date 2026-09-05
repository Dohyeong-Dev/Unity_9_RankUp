using System.Collections;
using UnityEngine;

/// <summary> 지정한 시간이 지나면 자신을 오브젝트 풀로 반환한다. </summary>
public class LifetimePoolObject : MonoBehaviour
{
    private Coroutine _lifetimeCoroutine;
    private PoolObj _poolObject;

    #region ===== 초기화 =====

    private void Awake()
    {
        _poolObject = GetComponent<PoolObj>();
    }

    #endregion ===== 초기화 =====

    #region ===== 수명 =====

    /// <summary> 지정한 시간 동안 활성화 상태를 유지한다. </summary>
    public void SetLifetime(float lifetime)
    {
        StopLifetimeCoroutine();

        _lifetimeCoroutine = StartCoroutine(Co_ReturnAfterTime(lifetime));
    }

    /// <summary> 지정한 시간이 지난 후 오브젝트 풀로 반환한다. </summary>
    private IEnumerator Co_ReturnAfterTime(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);

        _lifetimeCoroutine = null;

        ReturnToPool();
    }

    /// <summary> 현재 오브젝트를 소유 풀로 반환한다. </summary>
    private void ReturnToPool()
    {
        if (_poolObject != null)
        {
            Managers.Pool.Return(_poolObject);
            return;
        }

        gameObject.SetActive(false);
    }

    /// <summary> 실행 중인 수명 코루틴을 중지한다. </summary>
    private void StopLifetimeCoroutine()
    {
        if (_lifetimeCoroutine == null)
        {
            return;
        }

        StopCoroutine(_lifetimeCoroutine);
        _lifetimeCoroutine = null;
    }

    #endregion ===== 수명 =====

    private void OnDisable()
    {
        StopLifetimeCoroutine();
    }
}