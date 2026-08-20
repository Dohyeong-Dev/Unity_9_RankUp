using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerCtrl : MonoBehaviour
{
    #region Move

    [Header("Move")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _runSpeed = 8f;
    [SerializeField] private float _rotateSpeed = 720f;

    #endregion

    #region Jump

    [Header("Jump")]
    [SerializeField] private float _jumpHeight = 2f;
    [SerializeField] private float _wallFallSpeed = 2f;

    #endregion

    #region Wall

    [Header("Wall")]
    [Range(0f, 1f)]
    [Tooltip("이동 방향과 벽을 향하는 방향의 내적 기준값. 1에 가까울수록 정면으로 벽을 향한다.")]
    [SerializeField] private float _wallBlockThreshold = 0.5f;

    #endregion

    private CapsuleCollider _capsuleCollider;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private Camera _camera;

    private Vector2 _inputVector;

    private bool _jumpRequested;
    private bool _isJumping;
    private bool _isGrounded;
    private bool _isWallBlocked;

    private bool CanControl => Managers.UI.CurrentHUD?.IsInputEnabled ?? false;

    private void Awake()
    {
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    public void SetCamera(Camera camera)
    {
        _camera = camera;
    }

    private void Update()
    {
        if (!CanControl)
        {
            ClearInput();
            return;
        }

        UpdateInput();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!CanControl)
        {
            StopMovement();
            return;
        }

        _isGrounded = CheckGround();

        if (_isGrounded)
        {
            _isWallBlocked = false;
        }

        // 공중에서 벽에 부딪혔을 때는 이동을 막고 강제로 하강한다.
        if (!_isGrounded && _isWallBlocked)
        {
            ForceWallFall();
            return;
        }

        Vector3 moveDirection = GetMoveDirection();

        Move(moveDirection);
        Rotate(moveDirection);
        Jump();
    }

    #region Input

    private void UpdateInput()
    {
        _inputVector.x = Managers.Input.KeyAxisX;
        _inputVector.y = Managers.Input.KeyAxisY;

        if (Managers.Input.KeyDown_Space && !_isJumping)
        {
            _jumpRequested = true;
        }
    }

    private void ClearInput()
    {
        _inputVector = Vector2.zero;
        _jumpRequested = false;
    }

    private void StopMovement()
    {
        ClearInput();

        Vector3 velocity = _rigidbody.velocity;
        velocity.x = 0f;
        velocity.z = 0f;

        _rigidbody.velocity = velocity;
    }

    #endregion

    #region Move

    private Vector3 GetMoveDirection()
    {
        if (_camera == null)
        {
            return Vector3.zero;
        }

        Vector3 cameraForward = _camera.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = _camera.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 moveDirection = cameraRight * _inputVector.x + cameraForward * _inputVector.y;

        return moveDirection.sqrMagnitude > 1f ? moveDirection.normalized : moveDirection;
    }

    private void Move(Vector3 moveDirection)
    {
        bool isRunning = Managers.Input.Key_LeftShift;
        float moveSpeed = isRunning ? _runSpeed : _moveSpeed;

        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = _rigidbody.velocity.y;

        _rigidbody.velocity = velocity;
    }

    private void Rotate(Vector3 moveDirection)
    {
        if (moveDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        Quaternion rotation = Quaternion.RotateTowards(_rigidbody.rotation, targetRotation,
            _rotateSpeed * Time.fixedDeltaTime);
        _rigidbody.MoveRotation(rotation);
    }

    #endregion

    #region Jump

    private void Jump()
    {
        if (!_jumpRequested)
        {
            return;
        }

        _jumpRequested = false;

        // 공중에서는 점프하지 않는다.
        if (!_isGrounded || _isWallBlocked)
        {
            return;
        }

        // 1/2mv² = mgh
        float jumpVelocity = Mathf.Sqrt(2f * -Physics.gravity.y * _jumpHeight);

        Vector3 velocity = _rigidbody.velocity;
        velocity.y = jumpVelocity;

        _rigidbody.velocity = velocity;

        _isJumping = true;

        _animator.SetBool(AnimatorKey.Hash.IsGround, false);
        _animator.SetBool(AnimatorKey.Hash.IsJump, true);
        _animator.SetBool(AnimatorKey.Hash.IsFall, false);
    }

    private void ForceWallFall()
    {
        Vector3 velocity = _rigidbody.velocity;

        // 벽을 향한 수평 이동 차단
        velocity.x = 0f;
        velocity.z = 0f;

        // 일정한 속도로 강제 하강
        velocity.y = -_wallFallSpeed;

        _rigidbody.velocity = velocity;

        _isJumping = false;

        _animator.SetBool(AnimatorKey.Hash.IsGround, false);
        _animator.SetBool(AnimatorKey.Hash.IsJump, false);
        _animator.SetBool(AnimatorKey.Hash.IsFall, true);
    }

    #endregion

    #region Animation

    private void UpdateAnimation()
    {
        // 벽에 막힌 상태에서는 Fall을 우선한다.
        if (_isWallBlocked)
        {
            SetJumpAnimation(false, true);
            return;
        }

        bool isMoving = _inputVector.sqrMagnitude > 0.001f;
        bool isRunning = Managers.Input.Key_LeftShift;

        float animationSpeed = 0f;

        if (isMoving)
        {
            animationSpeed = isRunning ? 1f : 0.5f;
        }

        float dampingTime = _isGrounded && isMoving ? 0.15f : 0f;

        _animator.SetFloat(AnimatorKey.Hash.Speed, animationSpeed, dampingTime, Time.deltaTime);
        _animator.SetBool(AnimatorKey.Hash.IsGround, _isGrounded);

        bool isFalling = !_isGrounded && _isJumping && _rigidbody.velocity.y < 0f;

        _animator.SetBool(AnimatorKey.Hash.IsFall, isFalling);

        // 착지
        if (_isGrounded && _rigidbody.velocity.y <= 0f)
        {
            _isJumping = false;

            SetJumpAnimation(false, false);
        }
    }

    private void SetJumpAnimation(bool isJumping, bool isFalling)
    {
        _animator.SetBool(AnimatorKey.Hash.IsJump, isJumping);
        _animator.SetBool(AnimatorKey.Hash.IsFall, isFalling);
    }

    #endregion

    #region Ground

    private bool CheckGround()
    {
        // CapsuleCollider의 반지름을 기준으로 Ground를 검사한다.
        float radius = _capsuleCollider.bounds.extents.x;

        Vector3 origin = transform.position + Vector3.up * radius * 2f;
        Ray ray = new Ray(origin, Vector3.down);

        if (!Physics.SphereCast(ray, radius, out RaycastHit hit, radius + 0.1f, LayerKey.Mask.Floor))
        {
            return false;
        }

        // 위쪽을 향하는 면만 지면으로 인정
        return hit.normal.y > 0.5f;
    }

    #endregion

    #region Wall

    /// <summary> 충돌면의 법선 벡터와 이동 방향을 비교하여 벽을 향해 이동 중인지 판단한다. </summary>
    private bool IsMovingIntoWall(Collision collision)
    {
        Vector3 moveDirection = GetMoveDirection();

        if (moveDirection.sqrMagnitude < 0.001f)
        {
            return false;
        }

        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 wallNormal = contact.normal;

            // 수평면은 벽으로 취급하지 않는다.
            if (Mathf.Abs(wallNormal.y) > 0.5f)
            {
                continue;
            }

            float dot = Vector3.Dot(moveDirection, -wallNormal);

            if (dot >= _wallBlockThreshold)
            {
                return true;
            }
        }

        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryStartWallFall(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryStartWallFall(collision);
    }

    private void TryStartWallFall(Collision collision)
    {
        if (_isWallBlocked || !_isJumping)
        {
            return;
        }

        if (IsMovingIntoWall(collision))
        {
            StartWallFall();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!_isWallBlocked)
        {
            return;
        }

        // 현재 접촉 중인 다른 벽이 있다면 OnCollisionStay에서 다시 판정한다.
        _isWallBlocked = false;
    }

    private void StartWallFall()
    {
        _isWallBlocked = true;
        _isJumping = false;

        _animator.SetBool(AnimatorKey.Hash.IsGround, false);
        _animator.SetBool(AnimatorKey.Hash.IsJump, false);
        _animator.SetBool(AnimatorKey.Hash.IsFall, true);
    }

    #endregion
}