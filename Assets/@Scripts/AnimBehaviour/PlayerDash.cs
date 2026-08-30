using UnityEngine;

public class PlayerDash : StateMachineBehaviour
{
    private DashBehaviour _dash;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_dash == null && !animator.TryGetComponent(out _dash))
        {
            CPrint.Error("DashBehaviour no found!");

            return;
        }
        
        _dash.StartDashMovement();
    }
}