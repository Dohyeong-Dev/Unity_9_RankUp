using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponSocketEvent
{
    [Tooltip("State의 Exit Time까지를 0~1로 환산한 실행 Progress")]
    [Range(0f, 1f)]
    public float Progress;

    [Tooltip("실행할 Socket Event String")]
    public string SocketCommand;

    [HideInInspector]
    public bool Executed;
}

/// <summary> Animator State 진행률에 따라 무기 Socket 변경 이벤트를 실행한다. </summary>
public class WeaponSocketStateBehaviour : StateMachineBehaviour
{
    #region ===== 설정 =====

    [Header("⚠ Animator Transition Exit Time과 반드시 동일하게 설정")]
    [Tooltip("현재 State에서 나가는 Transition의 Exit Time과 동일하게 설정")]
    [Range(0.01f, 1f)]
    [SerializeField] private float _exitTime = 0.45f;

    [Header("Socket Events")]
    [SerializeField] private List<WeaponSocketEvent> _socketEvents = new();

    #endregion ===== 설정 =====

    #region ===== 참조 =====

    private Character_Weapon_Controller _weaponController;

    #endregion ===== 참조 =====

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        InitializeWeaponController(animator);
        ResetEvents();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_weaponController == null)
        {
            return;
        }

        float progress = GetProgress(stateInfo);
        UpdateSocketEvents(progress);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ResetEvents();
    }

    #region ===== 초기화 =====

    /// <summary> 현재 Animator에서 무기 컨트롤러 참조를 초기화한다. </summary>
    private void InitializeWeaponController(Animator animator)
    {
        if (_weaponController != null)
        {
            return;
        }

        _weaponController = animator.GetComponent<Character_Weapon_Controller>();

        if (_weaponController == null)
        {
            CPrint.Error("[WeaponSocketStateBehaviour] WeaponController를 찾을 수 없습니다.");
        }
    }

    /// <summary> 현재 State의 Socket Event 실행 여부를 모두 초기화한다. </summary>
    private void ResetEvents()
    {
        for (int i = 0; i < _socketEvents.Count; i++)
        {
            _socketEvents[i].Executed = false;
        }
    }

    #endregion ===== 초기화 =====

    #region ===== Socket Event =====

    /// <summary> 현재 진행률에 도달한 Socket Event를 실행한다. </summary>
    private void UpdateSocketEvents(float progress)
    {
        for (int i = 0; i < _socketEvents.Count; i++)
        {
            WeaponSocketEvent socketEvent = _socketEvents[i];

            if (socketEvent.Executed || progress < socketEvent.Progress)
            {
                continue;
            }

            ExecuteSocketEvent(socketEvent);
        }
    }

    /// <summary> 지정된 Socket Event를 실행한다. </summary>
    private void ExecuteSocketEvent(WeaponSocketEvent socketEvent)
    {
        socketEvent.Executed = true;
        _weaponController.SwitchSocketByString(socketEvent.SocketCommand);
    }

    /// <summary> 현재 State의 진행률을 Exit Time 기준 0~1로 변환한다. </summary>
    private float GetProgress(AnimatorStateInfo stateInfo)
    {
        return _exitTime > 0f ? Mathf.Clamp01(stateInfo.normalizedTime / _exitTime) : 0f;
    }

    #endregion ===== Socket Event =====
}