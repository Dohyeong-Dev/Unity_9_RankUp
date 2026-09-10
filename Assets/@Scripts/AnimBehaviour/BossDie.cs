using UnityEngine;

/// <summary> 보스 사망 애니메이션 종료 시 사망 완료 이벤트 호출 </summary>
public class BossDie : StateMachineBehaviour
{
    private bool _isFinished;

    public override void OnStateEnter(Animator animator,  AnimatorStateInfo stateInfo, int layerIndex)
    {
        _isFinished = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_isFinished || stateInfo.normalizedTime < 1f)
        {
            return;
        }

        _isFinished = true;

        Managers.UI.OpenLoadingUI(1f, () =>
        {
            Managers.UI.CloseLoadingUI();
            Managers.Event.RaiseBossClear();
        });
    }
}