using System.Collections;
using UnityEngine;

public class LifetimePoolObject : MonoBehaviour
{
    private Coroutine _lifeCoroutine;

    /// <summary> 오브젝트의 존재 시간을 설정합니다. </summary>
    public void SetLifetime(float lifetime)
    {
        if (_lifeCoroutine != null)
        {
            StopCoroutine(_lifeCoroutine);
        }

        gameObject.SetActive(true);

        _lifeCoroutine = StartCoroutine(DisableAfterTime(lifetime));
    }

    private IEnumerator DisableAfterTime(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);

        gameObject.SetActive(false);

        _lifeCoroutine = null;
    }

    private void OnDisable()
    {
        if (_lifeCoroutine != null)
        {
            StopCoroutine(_lifeCoroutine);
            _lifeCoroutine = null;
        }
    }
}