using System.Collections;
using UnityEngine;

/// <summary> 플레이어의 기본 이동, 회전, 달리기 및 발소리를 관리한다. </summary>
public class MoveBehaviour : BaseLocomotionBehaviour
{
    #region ===== 참조 =====
    
    private Transform _leftFoot;
    private Transform _rightFoot;
    
    #endregion ===== 참조 =====
    
    #region ===== 상태 =====

    private float _currentRunFactor = DefaultMoveFactor;

    private bool _canRunInCurrentState = true;

    private Coroutine _runStateCoroutine;

    // 발소리
    private enum Foot
    {
        Left,
        Right
    }

    private Foot _currentFoot = Foot.Right;

    private bool _hasLiftedFoot;
    private float _footDistance;

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

    [Header("발소리")]
    [Tooltip("발이 바닥에 닿았다고 판단하는 최대 거리")]
    [SerializeField] private float _footstepDistance = 0.12f;

    #endregion ===== 설정 =====

    private void Start()
    {
        InitializeFootTransforms();

        Player.SetDefLocomotionBehaviour(BehaviourHash);
    }

    public override void OnFixedUpdate()
    {
        if (!Managers.Input.CanReceivePlayer)
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

        if (Managers.Input.Key_LeftShift)
        {
            TryRun();
        }
        else if (Player.IsRunning)
        {
            SetPlayerRunState(false);
        }
    }

    #region ===== 초기화 =====

    /// <summary> 플레이어의 좌우 발 본 Transform을 초기화한다. </summary>
    private void InitializeFootTransforms()
    {
        _leftFoot = Player.Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        _rightFoot = Player.Animator.GetBoneTransform(HumanBodyBones.RightFoot);
    }

    #endregion ===== 초기화 =====

    #region ===== 이동 =====

    /// <summary> Rigidbody의 수평 이동 속도를 제거하고 이동 애니메이션을 정지한다. </summary>
    private void RemoveHorizontalVelocity()
    {
        Vector3 velocity = Player.Rigid.velocity;
        Player.Rigid.velocity = Vector3.up * velocity.y;

        SetPlayerRunState(false);
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

        float animationSpeed = inputMagnitude * _currentRunFactor;
        Player.Animator.SetFloat(AnimatorKey.Hash.Speed, animationSpeed, 0.01f, Time.fixedDeltaTime);

        UpdateFootstep();
    }

    #endregion ===== 이동 =====

    #region ===== 발소리 =====

    /// <summary> 현재 이동 중인 발의 위치를 확인하여 발소리를 재생한다. </summary>
    private void UpdateFootstep()
    {
        if (!Player.IsGrounded || Player.IsDashing || !Player.IsMoving)
        {
            return;
        }

        if (_leftFoot == null || _rightFoot == null)
        {
            return;
        }

        switch (_currentFoot)
        {
            case Foot.Left:
                UpdateFootDistance(_leftFoot, Foot.Right);
                break;

            case Foot.Right:
                UpdateFootDistance(_rightFoot, Foot.Left);
                break;
        }
    }

    /// <summary> 지정된 발이 올라갔다가 바닥에 내려오면 발소리를 재생한다. </summary>
    private void UpdateFootDistance(Transform footTransform, Foot nextFoot)
    {
        _footDistance = footTransform.position.y - Player.Tr.position.y;
        
        if (_footDistance > _footstepDistance)
        {
            _hasLiftedFoot = true;
            return;
        }

        if (!_hasLiftedFoot)
        {
            return;
        }

        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.FootStep, 0.5f, true, footTransform.position);

        _currentFoot = nextFoot;
        _hasLiftedFoot = false;
    }

    #endregion ===== 발소리 =====

    #region ===== 달리기 =====

    /// <summary> 현재 상태와 자원을 확인한 후 달리기를 시작한다. </summary>
    private void TryRun()
    {
        if (!_canRunInCurrentState)
        {
            return;
        }

        if (!Player.IsMoving)
        {
            return;
        }

        float spCost = RequiredSpRate * _runSpFactor * Time.fixedDeltaTime;

        if (!Player.HasEnoughSp(spCost))
        {
            SetPlayerRunState(false);
            return;
        }
        
        Player.SetSp(-spCost);
        SetPlayerRunState(true);
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

    /// <summary> 플레이어의 달리기 상태를 설정한다. </summary>
    private void SetPlayerRunState(bool isSet)
    {
        if (isSet)
        {
            Player.SetState(PlayerCtrl.PlayerState.Running);
            _currentRunFactor = _runFactor;
        }
        else
        {
            Player.UnsetState(PlayerCtrl.PlayerState.Running);
            _currentRunFactor = DefaultMoveFactor;
        }
    }
    
    #endregion ===== 달리기 =====
}