using System;

/// <summary> 게임 전역에서 사용하는 이벤트를 관리하고 발행한다. </summary>
public class EventManager
{
    #region ===== 이벤트 =====

    public event Action OnPhaseUpdated;
    public event Action OnPlayerDead;

    public event Action OnBossSpawned;
    
    #endregion ===== 이벤트 =====

    #region ===== 이벤트 발행 =====

    /// <summary> 페이즈가 갱신되었음을 알린다. </summary>
    public void RaisePhaseUpdated()
    {
        OnPhaseUpdated?.Invoke();
    }

    /// <summary> 플레이어가 사망했음을 알린다. </summary>
    public void RaisePlayerDead()
    {
        OnPlayerDead?.Invoke();
    }

    /// <summary> 보스가 등장했음을 알린다. </summary>
    public void RaiseBossSpawned()
    {
        OnBossSpawned?.Invoke();
    }
    
    #endregion ===== 이벤트 발행 =====
}