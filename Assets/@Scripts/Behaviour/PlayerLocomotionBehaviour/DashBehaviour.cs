using System.Collections;
using UnityEngine;

/// <summary> 플레이어의 대시 입력, 이동, 지속 시간 및 쿨타임을 관리한다. </summary>
[RequireComponent(typeof(PlayerCtrl))]
public class DashBehaviour : BaseLocomotionBehaviour
{
    #region ===== 상태 =====

    private float _dashDurationTimer;
    private float _dashCoolTimer;

    private Vector3 _dashDirection;

    private bool _canDash;
    private bool _canDashInCurrentState = true;
    private bool _isDashMovementStarted;

    private Coroutine _dashStateCoroutine;

    #endregion ===== 상태 =====

    #region ===== 설정 =====

    [Header("대시")]
    [SerializeField] private float _dashSpeed = 10f;
    [SerializeField] private float _dashDuration = 0.15f;
    [SerializeField] private float _dashCoolTime = 1f;

    #endregion ===== 설정 =====

    private void Start()
    {
        _dashCoolTimer = 0f;
        _canDash = true;
    }

    public override void OnFixedUpdate()
    {
        if (!Player.IsDashing || !_isDashMovementStarted)
        {
            return;
        }

        UpdateDashMove();
    }

    protected override void OnUpdateBeforeInput()
    {
        UpdateDashCoolTime();

        if (!Player.IsDashing || !_isDashMovementStarted)
        {
            return;
        }

        UpdateDashDuration();
    }

    protected override void OnUpdateAfterInput()
    {
        if (Managers.Input.KeyDown_Space || Managers.Input.MouseDown_Right)
        {
            TryDash();
        }
    }

    #region ===== 대시 =====

    /// <summary> 현재 상태와 자원을 확인한 후 대시를 시작한다. </summary>
    private void TryDash()
    {
        if (!Player.IsGrounded || !Player.IsDefaultBehaviour || Player.IsAttacking)
        {
            return;
        }

        if (!_canDash || !_canDashInCurrentState)
        {
            return;
        }

        if (!Player.HasEnoughSp(RequiredSpRate))
        {
            return;
        }

        _dashDirection = GetDashDirection();

        if (_dashDirection == Vector3.zero)
        {
            return;
        }

        Player.SetSp(-RequiredSpRate);

        _canDash = false;
        _dashCoolTimer = _dashCoolTime;
        _isDashMovementStarted = false;

        Player.SetCurLocomotionBehaviour(BehaviourHash);
        Player.SetState(PlayerCtrl.PlayerState.Dashing);
        Player.Animator.SetBool(AnimatorKey.Hash.IsDash, true);
    }

    /// <summary> Animator가 실제 Dash State에 진입했을 때 대시 이동과 지속 시간을 시작한다. </summary>
    public void StartDashMovement()
    {
        if (!Player.IsDashing || _isDashMovementStarted)
        {
            return;
        }

        _dashDurationTimer = _dashDuration;
        _isDashMovementStarted = true;
    }

    /// <summary> 대시 지속 시간을 감소시키고 시간이 끝나면 대시를 종료한다. </summary>
    private void UpdateDashDuration()
    {
        _dashDurationTimer -= Time.deltaTime;

        if (_dashDurationTimer > 0f)
        {
            return;
        }

        _dashDurationTimer = 0f;

        EndDash();
    }

    /// <summary> 현재 대시 방향으로 이동한다. </summary>
    private void UpdateDashMove()
    {
        Vector3 velocity = _dashDirection * _dashSpeed;
        Player.Rigid.velocity = new Vector3(velocity.x, Player.Rigid.velocity.y, velocity.z);
    }

    /// <summary> 플레이어가 바라보는 방향을 기준으로 대시 방향을 반환한다. </summary>
    private Vector3 GetDashDirection()
    {
        Vector3 direction = Player.Tr.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return Vector3.zero;
        }

        return direction.normalized;
    }

    /// <summary> 대시 쿨타임을 감소시키고 완료되면 다시 대시를 허용한다. </summary>
    private void UpdateDashCoolTime()
    {
        if (_dashCoolTimer <= 0f)
        {
            _canDash = true;
            return;
        }

        _dashCoolTimer -= Time.deltaTime;

        if (_dashCoolTimer > 0f)
        {
            return;
        }

        _dashCoolTimer = 0f;
        _canDash = true;
    }

    /// <summary> 현재 대시를 종료하고 플레이어 상태를 복구한다. </summary>
    public void EndDash()
    {
        if (!Player.IsDashing)
        {
            return;
        }

        _dashDurationTimer = 0f;
        _isDashMovementStarted = false;

        Player.UnsetState(PlayerCtrl.PlayerState.Dashing);
        Player.UnsetCurLocomotionBehaviour(BehaviourHash);
        Player.Animator.SetBool(AnimatorKey.Hash.IsDash, false);
    }

    #endregion ===== 대시 =====

    #region ===== 상태 제어 =====

    /// <summary> 현재 Animator State에서 대시 가능 여부를 설정한다. </summary>
    public void SetDashStateAllowed(bool allowed, float delay = 0f)
    {
        if (_canDashInCurrentState == allowed)
        {
            return;
        }

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

    /// <summary> 지정된 시간 이후 현재 State에서의 대시 가능 여부를 변경한다. </summary>
    private IEnumerator Co_SetDashStateAllowed(bool allowed, float delay)
    {
        yield return new WaitForSeconds(delay);

        _canDashInCurrentState = allowed;
        _dashStateCoroutine = null;
    }

    #endregion ===== 상태 제어 =====
}