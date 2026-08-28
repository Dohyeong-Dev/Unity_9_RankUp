using UnityEngine;

public class PlayerNoAttacking : StateMachineBehaviour
{
    private MoveBehaviour _moveBehaviour;

    [Header("대시")]
    [Tooltip("NoAttacking 진입 후 대시를 허용하기까지 기다리는 시간")]
    [SerializeField] private float _dashEnableDelay = 0.5f;

    [Header("달리기")]
    [Tooltip("NoAttacking 진입 후 달리기를 허용하기까지 기다리는 시간")]
    [SerializeField] private float _runEnableDelay = 0.5f;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_moveBehaviour == null)
        {
            _moveBehaviour = animator.GetComponent<MoveBehaviour>();
        }

        if (_moveBehaviour == null)
        {
            CPrint.Warning("[PlayerNoAttacking] MoveBehaviour를 찾을 수 없습니다.");
            return;
        }

        _moveBehaviour.SetDashStateAllowed(true, _dashEnableDelay);
        _moveBehaviour.SetRunStateAllowed(true, _runEnableDelay);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_moveBehaviour == null)
        {
            return;
        }

        _moveBehaviour.SetDashStateAllowed(false);
        _moveBehaviour.SetRunStateAllowed(false);
    }
}