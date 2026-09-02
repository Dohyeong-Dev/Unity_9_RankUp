using System;
using System.Collections.Generic;

public class EnemyStateMachine
{
    private readonly Dictionary<Type, BaseEnemyState> _stateMap = new();

    private BaseEnemyState _currentState;

    private float _stateElapsedTime;
    /// <summary> 현재 State에 머문 시간 </summary>
    public float StateElapsedTime => _stateElapsedTime;

    
    /// <summary> 현재 State를 업데이트합니다. </summary>
    public void UpdateCurrentState(float deltaTime)
    {
        if (_currentState == null)
        {
            return;
        }

        _stateElapsedTime += deltaTime;

        _currentState.Update(deltaTime);
    }

    /// <summary> State를 등록합니다. </summary>
    public void RegisterState(BaseEnemyState state)
    {
        if (state == null)
        {
            return;
        }

        Type stateType = state.GetType();

        if (_stateMap.ContainsKey(stateType))
        {
            CPrint.Warning($"이미 등록된 State입니다. [{stateType.Name}]");
            return;
        }

        _stateMap.Add(stateType, state);
    }

    /// <summary> 지정한 State로 전환합니다. </summary>
    public void ChangeState<T>() where T : BaseEnemyState
    {
        Type stateType = typeof(T);

        if (!_stateMap.TryGetValue(stateType, out BaseEnemyState nextState))
        {
            CPrint.Error($"등록되지 않은 State입니다. [{stateType.Name}]");
            return;
        }

        if (_currentState == nextState)
        {
            return;
        }

        _currentState?.Exit();

        _currentState = nextState;

        _stateElapsedTime = 0f;

        _currentState.Enter();
        
        CPrint.Success(_currentState.GetType().Name + "전환");
    }
}