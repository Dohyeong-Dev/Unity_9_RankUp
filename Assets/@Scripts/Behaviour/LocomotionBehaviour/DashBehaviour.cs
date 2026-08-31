using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerCtrl))]
public class DashBehaviour : BaseLocomotionBehaviour
{
    [Header("대시")]
    [SerializeField] private float _dashSpeed = 10f;
    [SerializeField] private float _dashDuration = 0.15f;
    [SerializeField] private float _dashCoolTime = 1f;

    private float _dashTimer;
    private float _dashCoolTimer;

    private Vector3 _dashDirection;

    private bool _canDash;
    private bool _canDashInCurrentState = true;
    private bool _isDashMovementStarted;

    private Coroutine _dashStateCoroutine;


    private void Start()
    {
        _dashCoolTimer = _dashCoolTime;
        _canDash = true;

        Player.AddLocomotionBehaviour(this);
    }

    public override void OnFixedUpdate()
    {
        if (!Player.IsDashing || !_isDashMovementStarted)
        {
            return;
        }

        UpdateDashMove();
    }

    protected override void OnUpdateAlways()
    {
        CalculateDashCoolTime();

        if (!Player.IsDashing || !_isDashMovementStarted)
        {
            return;
        }

        _dashTimer += Time.deltaTime;

        if (_dashTimer >= _dashDuration)
        {
            EndDash();
        }
    }

    protected override void OnUpdate()
    {
        if (Managers.Input.KeyDown_Space || Managers.Input.MouseDown_Right)
        {
            TryDash();
        }
    }

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
        _dashCoolTimer = 0f;
        _isDashMovementStarted = false;

        // 현재 Locomotion을 DashBehaviour로 변경
        Player.SetCurLocomotionBehaviour(BehaviourHash);

        // 대시 상태 시작
        Player.SetState(PlayerCtrl.PlayerState.Dashing);

        // Dash 애니메이션 재생
        Player.Animator.SetBool(AnimatorKey.Hash.IsDash, true);
    }

    /// <summary> Animator가 실제 Dash State에 진입했을 때 호출된다. 이 시점부터 실제 대시 이동과 대시 시간이 시작된다. </summary>
    public void StartDashMovement()
    {
        if (!Player.IsDashing || _isDashMovementStarted)
        {
            return;
        }

        _dashTimer = 0f;
        _isDashMovementStarted = true;
    }

    private void UpdateDashMove()
    {
        Vector3 velocity = _dashDirection * _dashSpeed;
        Player.Rigid.velocity = new Vector3(velocity.x, Player.Rigid.velocity.y, velocity.z);
    }

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

    public void EndDash()
    {
        if (!Player.IsDashing)
        {
            return;
        }

        _dashTimer = 0f;
        _isDashMovementStarted = false;

        Player.UnsetState(PlayerCtrl.PlayerState.Dashing);
        Player.UnsetCurLocomotionBehaviour(BehaviourHash);

        Player.Animator.SetBool(AnimatorKey.Hash.IsDash, false);
    }

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

    private IEnumerator Co_SetDashStateAllowed(bool allowed, float delay)
    {
        yield return new WaitForSeconds(delay);

        _canDashInCurrentState = allowed;
        _dashStateCoroutine = null;
    }
}