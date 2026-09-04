using System;

public class EventManager
{
    public event Action OnPhaseUpdated;
    public event Action OnPlayerDead;

    public void RaisePhaseUpdated()
    {
        OnPhaseUpdated?.Invoke();
    }

    public void RaisePlayerDead()
    {
        OnPlayerDead?.Invoke();
    }
}