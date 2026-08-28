using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponSocketEvent
{
    [Tooltip("State의 Exit Time까지를 0~1로 환산한 실행 Progress"), Range(0f, 1f)]
    public float Progress;

    [Tooltip("실행할 Socket Event String")]
    public string SocketCommand;

    [HideInInspector]
    public bool Executed;
}

public class WeaponSocketStateBehaviour : StateMachineBehaviour
{
    [Header("⚠ Animator Transition Exit Time과 반드시 동일하게 설정")]
    [Tooltip("현재 State에서 나가는 Transition의 Exit Time과 동일하게 설정")]
    [Range(0.01f, 1f)]
    [SerializeField] private float _exitTime = 0.45f;

    [Header("Socket Events")]
    [SerializeField] private List<WeaponSocketEvent> _socketEvents = new();

    private Character_Weapon_Controller _weaponController;

    public override void OnStateEnter(Animator animator,  AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_weaponController == null)
        {
            _weaponController = animator.GetComponent<Character_Weapon_Controller>();
        }

        ResetEvents();
    }

    public override void OnStateUpdate(Animator animator,  AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_weaponController == null)
        {
            CPrint.Error("WeaponController no found!");
            return;
        }

        float progress = GetProgress(stateInfo);

        for (int i = 0; i < _socketEvents.Count; i++)
        {
            WeaponSocketEvent socketEvent = _socketEvents[i];

            if (socketEvent.Executed)
            {
                continue;
            }

            if (progress < socketEvent.Progress)
            {
                continue;
            }

            ExecuteSocketEvent(socketEvent);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ResetEvents();
    }

    /// <summary> 현재 State의 진행률을 Exit Time 기준 0~1로 변환한다. </summary>
    private float GetProgress(AnimatorStateInfo stateInfo)
    {
        if (_exitTime <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(stateInfo.normalizedTime / _exitTime);
    }

    /// <summary> 지정된 Socket Event를 실행한다. </summary>
    private void ExecuteSocketEvent(WeaponSocketEvent socketEvent)
    {
        socketEvent.Executed = true;

        _weaponController.SwitchSocketByString(socketEvent.SocketCommand);
    }

    /// <summary> 현재 State의 Socket Event들의 실행여부를 모두 초기화한다. </summary>
    private void ResetEvents()
    {
        if (_socketEvents == null)
        {
            return;
        }

        for (int i = 0; i < _socketEvents.Count; i++)
        {
            _socketEvents[i].Executed = false;
        }
    }
}