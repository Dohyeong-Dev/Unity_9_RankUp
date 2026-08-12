using UnityEngine;

public abstract class BaseScene : MonoBehaviour
{
    protected abstract void OnAwake();

    private void Awake()
    {
        OnAwake();
    }
}
