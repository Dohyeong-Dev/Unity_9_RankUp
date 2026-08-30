using UnityEngine;

public class PlayerNoAttacking : StateMachineBehaviour
{
    private MoveBehaviour _moveBehaviour;
    private DashBehaviour _dashBehaviour;
    
    [Header("달리기")]
    [Tooltip("NoAttacking 진입 후 달리기를 허용하기까지 기다리는 시간")]
    [SerializeField] private float _runEnableDelay = 0.5f;
    
    [Header("대시")]
    [Tooltip("NoAttacking 진입 후 대시를 허용하기까지 기다리는 시간")]
    [SerializeField] private float _dashEnableDelay = 0.5f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_moveBehaviour == null && !animator.TryGetComponent(out _moveBehaviour))
        {
            CPrint.Error("MoveBehaviour no found!");
        }
        
        _moveBehaviour?.SetRunStateAllowed(true, _runEnableDelay);
        
        if (_dashBehaviour == null && !animator.TryGetComponent(out _dashBehaviour))
        {
            CPrint.Error("MoveBehaviour no found!");
        }
        
        _dashBehaviour?.SetDashStateAllowed(true, _dashEnableDelay);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_moveBehaviour != null)
        {
            _moveBehaviour.SetRunStateAllowed(false);
        }

        if (_dashBehaviour != null)
        {
            _dashBehaviour.SetDashStateAllowed(false);
        }
    }
}