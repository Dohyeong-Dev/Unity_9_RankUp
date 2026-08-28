using System.Collections;
using UnityEngine;

public class MoveBehaviour : BaseLocomotionBehaviour
{
    private const float DefaultMoveFactor = 1f;


    [Header("이동")]
    [SerializeField] private float _moveSpeed = 5f;


    [Header("대시")]
    [SerializeField] private float _dashSpeed = 2f;
    [SerializeField] private float _dashCoolTime = 1f;
    private float _dashCoolTimer;

    [Tooltip("대시 시 기본 SP 소비량에 적용되는 배율")]
    [SerializeField] private float _dashSpFactor = 5f;

    private Vector3 _dashDirection;

    private bool _canDash; // 대시 쿨타임이 끝났는가?
    private bool _canDashInCurrentState; // 현재 애니메이션 상태에서 대시가 허용되는가?

    private Coroutine _dashStateCoroutine;


    [Header("달리기 설정")]
    private float _currentRunFactor = DefaultMoveFactor;

    [Tooltip("달리기 시 기본 이동 속도에 적용되는 배율")]
    [SerializeField] private float _runFactor = 2f;

    [Tooltip("달리기 시 기본 SP 소비량에 적용되는 배율")]
    [SerializeField] private float _runSpFactor = 2f;

    private bool _canRunInCurrentState; // 현재 애니메이션 상태에서 달리기가 허용되는가?
    
    private Coroutine _runStateCoroutine;
    
    
    private void Start()
    {
        _dashCoolTimer = _dashCoolTime;

        Player.AddLocomotionBehaviour(this);
        Player.SetDefLocomotionBehaviour(BehaviourHash);
    }

    public override void OnFixedUpdate()
    {
        if (!Managers.Input.CanReceiveInput)
        {
            RemoveHorizontalVelocity();
            return;
        }

        if (Player.Rigid.velocity.y > 0f)
        {
            RemoveVerticalVelocity();
        }
        else if (Player.Rigid.velocity.y < -1f)
        {
            return;
        }

        UpdateRotate();
        UpdateMove();

        UpdateDash();
    }

    protected override void OnUpdateAlways()
    {
        if (Player.IsAttacking)
        {
            RemoveHorizontalVelocity();
        }

        CalculateDashCoolTime();
    }

    protected override void OnUpdate()
    {
        if (Player.IsAttacking)
        {
            return;
        }

        if (Managers.Input.KeyDown_Space)
        {
            TryDash();
        }

        AdjustRunSpeed();
    }

    private void RemoveHorizontalVelocity()
    {
        Vector3 velocity = Player.Rigid.velocity;
        Player.Rigid.velocity = Vector3.up * velocity.y;

        Player.Animator.SetFloat(AnimatorKey.Hash.Speed, 0f, 0.01f, Time.fixedDeltaTime);
    }

    private void RemoveVerticalVelocity()
    {
        Vector3 velocity = Player.Rigid.velocity;
        velocity.y = 0f;

        Player.Rigid.velocity = velocity;
    }

    private void UpdateRotate()
    {
        if (!Player.IsMoving)
        {
            return;
        }

        if (Player.Cam == null)
        {
            return;
        }

        Transform camTr = Player.Cam.transform;

        // 카메라의 수평 방향만 사용
        Vector3 forward = camTr.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        forward.Normalize();

        Vector3 right = camTr.right;
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

    private void UpdateMove()
    {
        // 이동 입력도 없고 대시도 아니라면 정지
        if (Managers.Input.KeyVecSqrMagnitude <= Mathf.Epsilon && !Player.IsDashing)
        {
            RemoveHorizontalVelocity();
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
            RemoveHorizontalVelocity();
            return;
        }

        moveDirection.Normalize();

        // 일반 이동 : 입력 크기에 따라 이동 속도 결정
        // 대시 : 입력이 없어도 항상 최대 대시 속도로 이동
        float movementInput = Player.IsDashing ? 1f : Managers.Input.KeyVecSqrMagnitude;
        Vector3 horizontalVelocity = moveDirection * speed * movementInput;
        Player.Rigid.velocity = new Vector3(horizontalVelocity.x, Player.Rigid.velocity.y, horizontalVelocity.z);

        if (Player.IsDashing)
        {
            return;
        }

        // 달리기 중에만 SP 소비
        ConsumeRunSp();

        // 실제 이동 속도에 맞춰 애니메이션 속도 조절
        float animationSpeed = movementInput * speedFactor;
        Player.Animator.SetFloat(AnimatorKey.Hash.Speed, animationSpeed, 0.01f, Time.fixedDeltaTime);
    }

    #region =====DASH=====

    private void TryDash()
    {
        if (!Player.IsDefaultBehaviour || !_canDash || !_canDashInCurrentState || !Player.IsGrounded)
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

        _canDash = false;

        _dashCoolTimer = 0f;

        Player.SetState(PlayerCtrl.PlayerState.Dashing);
    }

    private Vector3 GetDashDirection()
    {
        // 이동 입력이 있으면 카메라 기준 입력 방향으로 대시한다.
        if (Player.IsMoving)
        {
            if (Player.Cam != null)
            {
                Transform camTr = Player.Cam.transform;

                Vector3 forward = camTr.forward;
                forward.y = 0f;

                if (forward.sqrMagnitude > Mathf.Epsilon)
                {
                    forward.Normalize();

                    Vector3 right = camTr.right;
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
            _canDash = true;
            return;
        }

        _dashCoolTimer += Time.deltaTime;
        if (_dashCoolTimer >= _dashCoolTime)
        {
            _dashCoolTimer = _dashCoolTime;
            _canDash = true;
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

    public void SetDashStateAllowed(bool allowed, float delay = 0f)
    {
        if (_dashStateCoroutine != null)
        {
            StopCoroutine(_dashStateCoroutine);
            _dashStateCoroutine = null;
        }

        if (delay <= 0f)
        {
            _canDashInCurrentState = allowed;
            return;
        }

        _dashStateCoroutine = StartCoroutine(Co_SetDashStateAllowed(allowed, delay));
    }

    private IEnumerator Co_SetDashStateAllowed(bool allowed, float delay)
    {
        yield return new WaitForSeconds(delay);

        _canDashInCurrentState = allowed;
        _dashStateCoroutine = null;
    }

    #endregion =====DASH=====

    #region =====RUN=====

    private void ConsumeRunSp()
    {
        // 걷기에서는 SP를 소비하지 않는다.
        if (_currentRunFactor <= DefaultMoveFactor)
        {
            return;
        }

        // 대시는 TryDash에서 SP를 소비한다.
        if (Player.IsDashing)
        {
            return;
        }

        float spCost = RequiredSpRate * _runSpFactor * Time.fixedDeltaTime;
        Player.SetSp(-spCost);
    }

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

    public void SetRunStateAllowed(bool allowed, float delay = 0f)
    {
        if (_runStateCoroutine != null)
        {
            StopCoroutine(_runStateCoroutine);
            _runStateCoroutine = null;
        }

        if (delay <= 0f)
        {
            _canRunInCurrentState = allowed;
            return;
        }

        _runStateCoroutine = StartCoroutine(Co_SetRunStateAllowed(allowed, delay));
    }

    private IEnumerator Co_SetRunStateAllowed(bool allowed, float delay)
    {
        yield return new WaitForSeconds(delay);

        _canDashInCurrentState = allowed;
        _runStateCoroutine = null;
    }
    
    #endregion =====RUN=====
}