using System.Collections;
using UnityEngine;

public class MoveBehaviour : BaseLocomotionBehaviour
{
    private const float DefaultMoveFactor = 1f;

    [Header("이동")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("달리기 설정")]
    private float _currentRunFactor = DefaultMoveFactor;

    [Tooltip("달리기 시 기본 이동 속도에 적용되는 배율")]
    [SerializeField] private float _runFactor = 2f;

    [Tooltip("달리기 시 기본 SP 소비량에 적용되는 배율")]
    [SerializeField] private float _runSpFactor = 2f;

    private bool _canRunInCurrentState = true;
    private Coroutine _runStateCoroutine;


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
    }

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

        float speed = _moveSpeed * _currentRunFactor;

        Vector3 moveDirection = Player.Tr.forward;
        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            RemoveHorizontalVelocity();
            return;
        }

        moveDirection.Normalize();

        float movementInput = Managers.Input.KeyVecSqrMagnitude;
        Vector3 horizontalVelocity = moveDirection * speed * movementInput;

        Player.Rigid.velocity = new Vector3(horizontalVelocity.x, Player.Rigid.velocity.y, horizontalVelocity.z);

        ConsumeRunSp();

        float animationSpeed = movementInput * _currentRunFactor;
        Player.Animator.SetFloat(AnimatorKey.Hash.Speed, animationSpeed, 0.01f, Time.fixedDeltaTime);
    }


    #region =====RUN=====

    private void ConsumeRunSp()
    {
        if (_currentRunFactor <= DefaultMoveFactor)
        {
            return;
        }

        float spCost = RequiredSpRate * _runSpFactor * Time.fixedDeltaTime;
        Player.SetSp(-spCost);
    }

    private void AdjustRunSpeed()
    {
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

        _currentRunFactor = _runFactor;
    }

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

    private IEnumerator Co_SetRunStateAllowed(bool allowed, float delay)
    {
        yield return new WaitForSeconds(delay);

        _canRunInCurrentState = allowed;
        _runStateCoroutine = null;
    }

    #endregion =====RUN=====
}