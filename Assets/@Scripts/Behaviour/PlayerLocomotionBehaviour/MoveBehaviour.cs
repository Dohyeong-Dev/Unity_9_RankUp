using System.Collections;
using UnityEngine;

/// <summary> 플레이어의 기본 이동, 회전 및 달리기 상태를 관리한다. </summary>
public class MoveBehaviour : BaseLocomotionBehaviour
{
    #region ===== 상태 =====

    private float _currentRunFactor = DefaultMoveFactor;

    private bool _canRunInCurrentState = true;

    private Coroutine _runStateCoroutine;

    #endregion ===== 상태 =====

    #region ===== 설정 =====

    private const float DefaultMoveFactor = 1f;

    [Header("이동")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("달리기 설정")]
    [Tooltip("달리기 시 기본 이동 속도에 적용되는 배율")]
    [SerializeField] private float _runFactor = 2f;

    [Tooltip("달리기 시 기본 SP 소비량에 적용되는 배율")]
    [SerializeField] private float _runSpFactor = 2f;

    #endregion ===== 설정 =====

    private void Start()
    {
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
    }

    protected override void OnUpdateBeforeInput()
    {
        if (Player.IsAttacking)
        {
            RemoveHorizontalVelocity();
        }
    }

    protected override void OnUpdateAfterInput()
    {
        if (Player.IsAttacking)
        {
            return;
        }

        AdjustRunSpeed();
    }

    #region ===== 이동 =====

    /// <summary> Rigidbody의 수평 이동 속도를 제거하고 이동 애니메이션을 정지한다. </summary>
    private void RemoveHorizontalVelocity()
    {
        Vector3 velocity = Player.Rigid.velocity;
        Player.Rigid.velocity = Vector3.up * velocity.y;

        Player.UnsetState(PlayerCtrl.PlayerState.Running);
        Player.Animator.SetFloat(AnimatorKey.Hash.Speed, 0f, 0.01f, Time.fixedDeltaTime);
    }

    /// <summary> Rigidbody의 수직 이동 속도를 제거한다. </summary>
    private void RemoveVerticalVelocity()
    {
        Vector3 velocity = Player.Rigid.velocity;
        velocity.y = 0f;

        Player.Rigid.velocity = velocity;
    }

    /// <summary> 현재 카메라와 입력 방향을 기준으로 플레이어의 회전을 갱신한다. </summary>
    private void UpdateRotate()
    {
        if (!Player.IsMoving || Player.Cam == null)
        {
            return;
        }

        Transform camTransform = Player.Cam.transform;

        Vector3 forward = camTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        forward.Normalize();

        Vector3 right = camTransform.right;
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
    }

    /// <summary> 현재 입력과 이동 상태를 기준으로 플레이어의 이동 속도를 갱신한다. </summary>
    private void UpdateMove()
    {
        if (Managers.Input.KeyVecSqrMagnitude <= Mathf.Epsilon)
        {
            RemoveHorizontalVelocity();
            return;
        }

        if (_currentRunFactor > DefaultMoveFactor && !Player.CanUseStamina)
        {
            _currentRunFactor = DefaultMoveFactor;
        }

        Vector3 moveDirection = Player.Tr.forward;
        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            RemoveHorizontalVelocity();
            return;
        }

        moveDirection.Normalize();

        float speed = _moveSpeed * _currentRunFactor;
        float inputMagnitude = Managers.Input.KeyVecSqrMagnitude;

        Vector3 horizontalVelocity = moveDirection * speed * inputMagnitude;

        Player.Rigid.velocity = new Vector3(horizontalVelocity.x, Player.Rigid.velocity.y, horizontalVelocity.z);

        ConsumeRunSp();

        float animationSpeed = inputMagnitude * _currentRunFactor;
        Player.Animator.SetFloat(AnimatorKey.Hash.Speed, animationSpeed, 0.01f, Time.fixedDeltaTime);
    }

    #endregion ===== 이동 =====

    #region ===== 달리기 =====

    /// <summary> 달리는 동안 매 FixedUpdate마다 SP를 소비한다. </summary>
    private void ConsumeRunSp()
    {
        if (_currentRunFactor <= DefaultMoveFactor)
        {
            return;
        }

        float spCost = RequiredSpRate * _runSpFactor * Time.fixedDeltaTime;
        Player.SetSp(-spCost);
    }

    /// <summary> 현재 입력과 상태를 기준으로 달리기 상태와 이동 배율을 갱신한다. </summary>
    private void AdjustRunSpeed()
    {
        Player.UnsetState(PlayerCtrl.PlayerState.Running);

        _currentRunFactor = DefaultMoveFactor;

        if (!_canRunInCurrentState)
        {
            return;
        }

        if (!Managers.Input.Key_LeftShift || !Player.IsMoving)
        {
            return;
        }

        if (!Player.CanUseStamina)
        {
            return;
        }

        Player.SetState(PlayerCtrl.PlayerState.Running);
        _currentRunFactor = _runFactor;
    }

    /// <summary> 현재 State에서 달리기 가능 여부를 설정한다. </summary>
    public void SetRunStateAllowed(bool allowed, float delay = 0f)
    {
        if (_canRunInCurrentState == allowed)
        {
            return;
        }

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

    /// <summary> 지정된 시간 이후 현재 State에서의 달리기 가능 여부를 변경한다. </summary>
    private IEnumerator Co_SetRunStateAllowed(bool allowed, float delay)
    {
        yield return new WaitForSeconds(delay);

        _canRunInCurrentState = allowed;
        _runStateCoroutine = null;
    }

    #endregion ===== 달리기 =====
}