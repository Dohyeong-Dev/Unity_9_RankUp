using UnityEngine;

public class MoveBehaviour : BaseLocomotionBehaviour
{
    [Header("이동 설정")] 
    [SerializeField] private float _moveSpeed = 5f;

    [Header("대시 설정")] 
    [SerializeField] private float _dashSpeed = 2f;
    [SerializeField] private float _dashCoolTime = 1f;
    private float _dashCoolTimer;
    [Tooltip("대시 시 기본 SP 소비량에 적용되는 배율")] 
    [SerializeField] private float _dashSpFactor = 5f;
    private Vector3 _dashDirection; // 대시 시작 순간에 결정되는 방향

    [Header("달리기 설정")] 
    [Tooltip("달리기 시 기본 이동 속도에 적용되는 배율")] 
    [SerializeField] private float _runFactor = 2f;

    [Tooltip("달리기 시 기본 SP 소비량에 적용되는 배율")]
    [SerializeField] private float _runSpFactor = 2f;

    private const float DefaultMoveFactor = 1f;

    private float _currentRunFactor = DefaultMoveFactor;

    private void Start()
    {
        _dashCoolTimer = _dashCoolTime;

        Player.AddBehaviour(this);
        Player.SetDefLocomotionBehaviour(BehaviourHash);
    }

    public override void OnFixedUpdate()
    {
        if (!Player.CanControl)
        {
            StopHorizontalMovement();
            return;
        }

        if (!Player.Animator.GetBool(AnimatorKey.Hash.IsJump) && Player.Rigid.velocity.y > 0f)
        {
            RemoveVerticalVelocity();
        }
        else if (Player.Rigid.velocity.y < -1f)
        {
            Player.Animator.SetFloat(AnimatorKey.Hash.Speed, 0f);

            return;
        }

        UpdateRotate();
        UpdateMove();
        UpdateDash();
    }

    private void RemoveVerticalVelocity()
    {
        Vector3 velocity = Player.Rigid.velocity;
        velocity.y = 0f;

        Player.Rigid.velocity = velocity;
    }
    
    protected override void OnUpdateAlways()
    {
        CalculateDashCoolTime();
    }

    protected override void OnUpdate()
    {
        if (Managers.Input.MouseDown_Right)
        {
            TryDash();
        }

        AdjustRunSpeed();
    }

    #region Rotate

    private void UpdateRotate()
    {
        if (!Player.IsMoving)
        {
            return;
        }

        Camera camera = Player.Cam;
        if (camera == null)
        {
            return;
        }

        // 카메라의 수평 방향만 사용
        Vector3 forward = camera.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }
        forward.Normalize();

        Vector3 right = camera.transform.right;
        right.y = 0f;
        if (right.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }
        right.Normalize();

        Vector3 targetDirection = right * Managers.Input.KeyAxisX + forward * Managers.Input.KeyAxisY;
        if (targetDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }
        targetDirection.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion viewRotation = Quaternion.Slerp(Player.Tr.rotation, targetRotation, Player.RotationSlerpFactor);

        Player.Rigid.MoveRotation(viewRotation);
        Player.SetLastDirection(targetDirection);
    }

    #endregion

    #region Move

    private void UpdateMove()
    {
        float inputMagnitude = Managers.Input.KeyVecMagnitude;

        // 이동 입력도 없고 대시도 아니라면 정지
        if (inputMagnitude <= Mathf.Epsilon && !Player.IsDashing)
        {
            StopHorizontalMovement();
            return;
        }

        // SP가 부족하면 달리기만 걷기로 전환한다. 그리고 기본 이동 자체는 SP를 소비하지 않는다.
        if (_currentRunFactor > DefaultMoveFactor && !Player.CanUseStamina)
        {
            _currentRunFactor = DefaultMoveFactor;
        }

        float speedFactor = Player.IsDashing ? _dashSpeed : _currentRunFactor;
        float speed = _moveSpeed * speedFactor;

        Vector3 moveDirection;
        if (Player.IsDashing)
        {
            // 대시는 시작 순간에 결정한 방향을 사용한다.
            moveDirection = _dashDirection;
        }
        else
        {
            // 일반 이동은 현재 캐릭터가 바라보는 방향으로 이동한다.
            moveDirection = Player.Tr.forward;
            moveDirection.y = 0f;
        }

        if (moveDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            StopHorizontalMovement();
            return;
        }

        moveDirection.Normalize();

        // 일반 이동 : 입력 크기에 따라 이동 속도 결정
        // 대시 : 입력이 없어도 항상 최대 대시 속도로 이동
        float movementInput = Player.IsDashing ? 1f : inputMagnitude;
        Vector3 horizontalVelocity = moveDirection * speed * movementInput;
        Player.Rigid.velocity = new Vector3(horizontalVelocity.x, Player.Rigid.velocity.y, horizontalVelocity.z);

        // 달리기 중에만 SP 소비
        ConsumeRunSp();

        // 실제 이동 속도에 맞춰 애니메이션 속도 조절
        float animationSpeed = movementInput * speedFactor;
        Player.Animator.SetFloat(AnimatorKey.Hash.Speed, animationSpeed, 0.01f, Time.fixedDeltaTime);
    }

    private void StopHorizontalMovement()
    {
        Vector3 velocity = Player.Rigid.velocity;
        Player.Rigid.velocity = Vector3.up * velocity.y;

        Player.Animator.SetFloat(AnimatorKey.Hash.Speed, 0f, 0.01f, Time.fixedDeltaTime);
    }

    private void ConsumeRunSp()
    {
        // 대시는 시작할 때 SP를 한 번 소비한다.
        if (Player.IsDashing)
        {
            return;
        }

        // 걷기에서는 SP를 소비하지 않는다.
        if (_currentRunFactor <= DefaultMoveFactor)
        {
            return;
        }

        float spCost = RequiredSpRate * _runSpFactor * Time.fixedDeltaTime;
        Player.SetSp(-spCost);
    }

    #endregion

    #region Dash

    private void TryDash()
    {
        if (!Player.IsDefaultBehaviour || !Player.CanDash || !Player.IsGrounded)
        {
            return;
        }

        float requiredSp = RequiredSpRate * _dashSpFactor;

        // SP가 부족하면 대시하지 않는다.
        if (!Player.HasEnoughSp(requiredSp))
        {
            return;
        }

        // 대시 방향을 대시 시작 순간에 결정한다.
        _dashDirection = GetDashDirection();

        if (_dashDirection == Vector3.zero)
        {
            return;
        }

        Player.SetSp(-requiredSp);

        Player.SetCanDash(false);

        _dashCoolTimer = 0f;

        Player.SetState(PlayerCtrl.PlayerState.Dashing);
    }
    
    private Vector3 GetDashDirection()
    {
        // 이동 입력이 있으면 카메라 기준 입력 방향으로 대시한다.
        if (Player.IsMoving)
        {
            Camera camera = Player.Cam;

            if (camera != null)
            {
                Vector3 forward = camera.transform.forward;
                forward.y = 0f;

                if (forward.sqrMagnitude > Mathf.Epsilon)
                {
                    forward.Normalize();

                    Vector3 right = camera.transform.right;
                    right.y = 0f;
                    if (right.sqrMagnitude > Mathf.Epsilon)
                    {
                        right.Normalize();

                        Vector3 direction = right * Managers.Input.KeyAxisX + forward * Managers.Input.KeyAxisY;
                        if (direction.sqrMagnitude > Mathf.Epsilon)
                        {
                            return direction.normalized;
                        }
                    }
                }
            }
        }

        // 이동 입력이 없다면 현재 캐릭터가 바라보는 방향으로 대시한다.
        Vector3 characterForward = Player.Tr.forward;
        characterForward.y = 0f;
        if (characterForward.sqrMagnitude <= Mathf.Epsilon)
        {
            return Vector3.zero;
        }

        return characterForward.normalized;
    }

    private void CalculateDashCoolTime()
    {
        if (_dashCoolTimer >= _dashCoolTime)
        {
            Player.SetCanDash(true);
            return;
        }

        _dashCoolTimer += Time.deltaTime;
        if (_dashCoolTimer >= _dashCoolTime)
        {
            _dashCoolTimer = _dashCoolTime;
            Player.SetCanDash(true);
        }
    }

    private void UpdateDash()
    {
        if (!Player.IsDashing)
        {
            return;
        }

        if (!Player.Animator.GetBool(AnimatorKey.Hash.IsDash) && Player.IsGrounded)
        {
            CancelInvoke(nameof(DashOut));

            Player.Animator.SetBool(AnimatorKey.Hash.IsDash, true);

            Invoke(nameof(DashOut), 0.15f);
        }
    }

    private void DashOut()
    {
        Player.UnsetState(PlayerCtrl.PlayerState.Dashing);

        Player.Animator.SetBool(AnimatorKey.Hash.IsDash, false);
    }

    #endregion

    #region Run

    private void AdjustRunSpeed()
    {
        // 기본값은 걷기
        _currentRunFactor = DefaultMoveFactor;

        // 달리기를 누르지 않았거나 이동하지 않으면 걷기
        if (!Managers.Input.Key_LeftShift || !Player.IsMoving)
        {
            return;
        }

        // SP가 없으면 달리기만 사용할 수 없다. 걷기는 계속 가능하다.
        if (!Player.CanUseStamina)
        {
            return;
        }

        // 달리기
        _currentRunFactor = _runFactor;
    }

    #endregion
}