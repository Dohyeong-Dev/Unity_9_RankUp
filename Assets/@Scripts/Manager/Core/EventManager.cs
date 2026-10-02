using System;

/// <summary> 게임 전역에서 사용하는 이벤트를 관리하고 발행한다. </summary>
public class EventManager
{
    #region ===== 이벤트 =====

    public event Action OnPhaseUpdated;
    public event Action OnPlayerDead;

    public event Action OnBossSpawned;
    public event Action OnBossClear;

    public event Action OnPause;
    public event Action OnResume;

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

    /// <summary> 보스를 클리어했음을 알린다. </summary>
    public void RaiseBossClear()
    {
        OnBossClear?.Invoke();
    }

    /// <summary> 게임이 일시정지되었음을 알린다. </summary>
    public void RaisePause()
    {
        OnPause?.Invoke();
    }

    /// <summary> 게임이 일시정지 상태에서 해제되었음을 알린다. </summary>
    public void RaiseResume()
    {
        OnResume?.Invoke();
    }

    #endregion ===== 이벤트 발행 =====
}