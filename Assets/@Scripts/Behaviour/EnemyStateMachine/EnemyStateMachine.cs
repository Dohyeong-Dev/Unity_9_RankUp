using System;
using System.Collections.Generic;

/// <summary> 적의 상태를 등록하고 현재 상태의 전환과 갱신을 관리한다. </summary>
public class EnemyStateMachine
{
    #region ===== 상태 =====

    private readonly Dictionary<Type, BaseEnemyState> _stateMap = new();

    private BaseEnemyState _currentState;

    #endregion ===== 상태 =====

    #region ===== 상태 시간 =====

    private float _stateElapsedTime;

    /// <summary> 현재 상태에 진입한 후 경과한 시간을 반환한다. </summary>
    public float StateElapsedTime => _stateElapsedTime;

    #endregion ===== 상태 시간 =====

    #region ===== 상태 등록 =====

    /// <summary> 상태 머신에서 사용할 상태를 등록한다. </summary>
    public void RegisterState(BaseEnemyState state)
    {
        if (state == null)
        {
            CPrint.Warning("[EnemyStateMachine] 등록할 State가 null입니다.");
            return;
        }

        Type stateType = state.GetType();

        if (_stateMap.ContainsKey(stateType))
        {
            CPrint.Warning($"[EnemyStateMachine] 이미 등록된 State입니다. [{stateType.Name}]");
            return;
        }

        _stateMap.Add(stateType, state);
    }

    #endregion ===== 상태 등록 =====

    #region ===== 상태 갱신 =====

    /// <summary> 현재 상태의 경과 시간을 갱신하고 Update를 호출한다. </summary>
    public void UpdateCurrentState(float deltaTime)
    {
        if (_currentState == null)
        {
            CPrint.Warning("[EnemyStateMachine] currentState가 존재하지 않습니다.");
            return;
        }

        _stateElapsedTime += deltaTime;

        _currentState.Update(deltaTime);
    }

    #endregion ===== 상태 갱신 =====

    #region ===== 상태 전환 =====

    /// <summary> 지정한 타입의 상태로 전환한다. </summary>
    public void ChangeState<T>() where T : BaseEnemyState
    {
        Type stateType = typeof(T);

        if (!_stateMap.TryGetValue(stateType, out BaseEnemyState nextState))
        {
            CPrint.Error($"[EnemyStateMachine] 등록되지 않은 State입니다. [{stateType.Name}]");
            return;
        }

        if (_currentState == nextState)
        {
            CPrint.Warning($"{nextState?.Enemy.name} : {_currentState} => {nextState}");
            return;
        }

        _currentState?.Exit();

        //CPrint.Log($"{nextState?.Enemy.name} : {_currentState} => {nextState}");
        _currentState = nextState;
        _stateElapsedTime = 0f;

        _currentState.Enter();
    }

    /// <summary> 현재 상태를 종료한 후 처음부터 다시 시작한다. </summary>
    public void RestartCurrentState()
    {
        if (_currentState == null)
        {
            return;
        }

        _currentState.Exit();

        _stateElapsedTime = 0f;

        _currentState.Enter();
    }

    #endregion ===== 상태 전환 =====

    #region ===== 상태 확인 =====

    /// <summary> 현재 상태가 지정한 타입인지 확인한다. </summary>
    public bool IsCurrentState<T>() where T : BaseEnemyState
    {
        return _currentState is T;
    }

    /// <summary> 현재 상태를 지정한 타입으로 가져온다. </summary>
    public bool TryGetCurrentState<T>(out T enemyState) where T : BaseEnemyState
    {
        enemyState = _currentState as T;

        return enemyState != null;
    }

    #endregion ===== 상태 확인 =====
}