using UnityEngine;

/// <summary> 자신이 소속된 오브젝트 풀의 정보를 관리한다. </summary>
public class PoolObj : MonoBehaviour
{
    /// <summary> 자신이 소속된 오브젝트 풀의 식별자다. </summary>
    public string PoolKey { get; private set; }

    /// <summary> 자신이 소속된 오브젝트 풀의 식별자를 설정한다. </summary>
    public void SetPoolKey(string poolKey)
    {
        PoolKey = poolKey;
    }
}