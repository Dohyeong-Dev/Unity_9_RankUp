public enum InteractType
{
    PickUp,
}

/// <summary> 플레이어와 상호작용할 수 있는 대상의 공통 규격을 정의한다. </summary>
public interface IInteractable
{
    InteractType Type { get; }
    
    void Interact();
}