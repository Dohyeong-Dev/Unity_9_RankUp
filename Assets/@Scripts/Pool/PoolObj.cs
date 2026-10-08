using UnityEngine;

/// <summary> 자신이 소속된 오브젝트 풀의 정보를 관리한다. </summary>
public class PoolObj : MonoBehaviour
{
    private Transform _originalParent;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Vector3 _originalScale;

    /// <summary> 자신이 소속된 오브젝트 풀의 식별자다. </summary>
    public string PoolKey { get; private set; }

    private void Awake()
    {
        _originalParent = transform.parent;
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
        _originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        transform.SetParent(_originalParent);
        transform.position = _originalPosition;
        transform.rotation = _originalRotation;
        transform.localScale = _originalScale;
    }

    /// <summary> 자신이 소속된 오브젝트 풀의 식별자를 설정한다. </summary>
    public void SetPoolKey(string poolKey)
    {
        PoolKey = poolKey;
    }

    /// <summary> 지정한 오브젝트를 부모로 설정하여 함께 따라가게 한다. </summary>
    public void AttachTo(Transform target)
    {
        if (target == null)
        {
            return;
        }

        transform.SetParent(target);
        transform.localPosition = Vector3.zero;
    }
}