using UnityEngine;

/// <summary> 플레이어가 상호작용하여 획득할 수 있는 Gold를 처리한다. </summary>
public class Gold : MonoBehaviour, IInteractable
{
    public InteractType Type => InteractType.PickUp;

    private int _amount;
    
    /// <summary> Gold를 획득하고 오브젝트를 제거한다. </summary>
    public void Interact()
    {
        Managers.Data.AddGold(_amount);
        Destroy(gameObject);
    }
    
    /// <summary> 골드량을 설정한다. </summary>
    public void SetGold(int amount)
    {
        _amount = amount;
    }
}