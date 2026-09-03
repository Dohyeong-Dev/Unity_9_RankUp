using System;
using System.Collections.Generic;

public class EnemyStateMachine
{
    private readonly Dictionary<Type, BaseEnemyState> _stateMap = new();

    private BaseEnemyState _currentState;

    private float _stateElapsedTime;
    public float StateElapsedTime => _stateElapsedTime;


    public void UpdateCurrentState(float deltaTime)
    {
        if (_currentState == null)
        {
            return;
        }

        _stateElapsedTime += deltaTime;

        _currentState.Update(deltaTime);
    }

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
    }

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

    public bool IsCurrentState<T>() where T : BaseEnemyState
    {
        return _currentState is T;
    }

    public bool TryGetCurrentState<T>(out T enemyState) where T : BaseEnemyState
    {
        enemyState = _currentState as T;

        return enemyState != null;
    }
}