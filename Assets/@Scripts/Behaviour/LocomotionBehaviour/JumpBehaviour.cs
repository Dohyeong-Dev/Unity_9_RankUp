using UnityEngine;

public class JumpBehaviour : BaseLocomotionBehaviour
{
    [Header("점프 설정")]
    [Tooltip("점프 높이")]
    [SerializeField] private float _jumpHeight;
    [Tooltip("공중에서 바라보는 방향으로 이동하는 가속도 배율")]
    [SerializeField] private float _airMoveFactor;
    [Tooltip("낙하 중 적용되는 중력 배율")]
    [SerializeField] private float _fallGravityFactor = 2.5f;

    private void Start()
    {
        Player.AddLocomotionBehaviour(this);
    }

    public override void OnFixedUpdate()
    {
        Jump();
    }

    protected override void OnUpdate()
    {
        if (Managers.Input.KeyDown_Space)
        {
            TryJump();
        }

        // 이미 점프 상태라면 상태 처리는 계속 유지
        if (Player.IsCurLocomotionBehaviour(BehaviourHash))
        {
            Player.SetState(PlayerCtrl.PlayerState.Jumping);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        Player.SetState(PlayerCtrl.PlayerState.Colliding);
    }

    private void OnCollisionExit(Collision collision)
    {
        Player.UnsetState(PlayerCtrl.PlayerState.Colliding);
    }

    private void Jump()
    {
        // 점프 시작
        if (!Player.Animator.GetBool(AnimatorKey.Hash.IsJump) && Player.IsGrounded && Player.IsJumping)
        {
            Player.CapsuleCollider.material.staticFriction = 0f;
            Player.CapsuleCollider.material.dynamicFriction = 0f;

            Player.Animator.SetBool(AnimatorKey.Hash.IsJump, true);

            Vector3 jumpVelocity = Vector3.up * Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * _jumpHeight);

            Player.Rigid.AddForce(jumpVelocity, ForceMode.VelocityChange);
        }
        // 점프 진행
        else if (Player.Animator.GetBool(AnimatorKey.Hash.IsJump))
        {
            // 낙하 중 추가 중력 적용
            if (Player.Rigid.velocity.y < 0f)
            {
                Player.Rigid.AddForce(Physics.gravity * (_fallGravityFactor - 1f), ForceMode.Acceleration);
            }

            // 공중에서 바라보는 방향으로 이동
            if (!Player.IsGrounded && !Player.IsColliding)
            {
                Player.Rigid.AddForce(Player.Tr.forward * Mathf.Abs(Physics.gravity.y) * _airMoveFactor, ForceMode.Acceleration);
            }

            // 착지
            if (Player.Rigid.velocity.y < Mathf.Epsilon && Player.IsGrounded)
            {
                Player.CapsuleCollider.material.staticFriction = 0.6f;
                Player.CapsuleCollider.material.dynamicFriction = 0.6f;

                Player.Animator.SetBool(AnimatorKey.Hash.IsGround, true);
                Player.Animator.SetBool(AnimatorKey.Hash.IsJump, false);

                Player.UnsetState(PlayerCtrl.PlayerState.Jumping);

                Player.UnsetCurLocomotionBehaviour(BehaviourHash);
            }
        }
    }

    public void TryJump()
    {
        if (!Player.IsDefaultBehaviour || !Player.IsGrounded)
        {
            return;
        }

        if (Player.SP < RequiredSpRate)
        {
            return;
        }

        Player.SetSp(-RequiredSpRate);

        Player.SetCurLocomotionBehaviour(BehaviourHash);
    }
}