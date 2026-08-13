using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Animator))]
public class PlayerCtrl : MonoBehaviour
{
    [Header("Move")] [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _runSpeed = 8f;
    [SerializeField] private float _rotateSpeed = 720f;

    [Header("Jump")] [SerializeField] private float _jumpHeight = 2f;
    [SerializeField] private float _wallFallSpeed = 2f;

    [Header("Wall")]
    [Range(0f, 1f), Tooltip("이동 방향과 벽을 향하는 방향의 내적 기준값. 1에 가까울수록 정면으로 벽을 향한다.")]
    [SerializeField] private float _wallBlockThreshold = 0.5f;

    private CapsuleCollider _capsuleCollider;
    private Rigidbody _rigidbody;
    private Animator _animator;

    private Camera _camera;

    private Vector2 _inputVec;

    private bool _jumpRequested;

    private bool _isJumping;
    private bool _isGround;
    private bool _isWallBlocked;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _capsuleCollider = GetComponent<CapsuleCollider>();

        if (_capsuleCollider == null)
        {
            CPrint.Error("CapsuleCollider is null!");
        }
    }

    public void SetCamera(Camera cam)
    {
        _camera = cam;
    }

    private void Update()
    {
        _inputVec.x = Managers.Input.KeyAxisX;
        _inputVec.y = Managers.Input.KeyAxisY;

        if (Managers.Input.KeyDown_Space && !_isJumping)
        {
            _jumpRequested = true;
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        _isGround = CheckGround();

        if (_isGround)
        {
            _isWallBlocked = false;
        }

        // 공중에서 벽에 부딪혔을 때는 움직이는 것을 막고 강제로 하강한다.
        if (!_isGround && _isWallBlocked)
        {
            ForceWallFall();
            return;
        }

        Vector3 moveDir = GetMoveDir();
        Move(moveDir);
        Rotate(moveDir);
        Jump();
    }

    #region Move

    private Vector3 GetMoveDir()
    {
        Vector3 camForwardVec = _camera.transform.forward;
        camForwardVec.y = 0f;
        Vector3 camForwardDir = camForwardVec.normalized;

        Vector3 camRightVec = _camera.transform.right;
        camRightVec.y = 0f;
        Vector3 camRightDir = camRightVec.normalized;

        Vector3 moveVec = camRightDir * _inputVec.x + camForwardDir * _inputVec.y;
        return moveVec.sqrMagnitude > 1f ? moveVec.normalized : moveVec;
    }

    private void Move(Vector3 moveDir)
    {
        bool isRunning = Managers.Input.Key_LeftShift;
        float moveSpeed = isRunning ? _runSpeed : _moveSpeed;
        Vector3 velocityVec = moveDir * moveSpeed;
        velocityVec.y = _rigidbody.velocity.y;

        _rigidbody.velocity = velocityVec;
    }

    private void Rotate(Vector3 dirVec)
    {
        if (dirVec.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(dirVec);
        transform.rotation = Quaternion.RotateTowards(_rigidbody.rotation, targetRotation,
            _rotateSpeed * Time.fixedDeltaTime);
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
        if (!_isGround || _isWallBlocked)
        {
            return;
        }

        // 1/2mv^2 = mgh
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
            _animator.SetBool(AnimatorKey.Hash.IsGround, false);
            _animator.SetBool(AnimatorKey.Hash.IsJump, false);
            _animator.SetBool(AnimatorKey.Hash.IsFall, true);

            return;
        }

        bool isMoving = _inputVec.sqrMagnitude > 0.001f;
        bool isRunning = Managers.Input.Key_LeftShift;

        float speed = 0f;

        if (isMoving)
        {
            speed = isRunning ? 1f : 0.5f;
        }

        _animator.SetFloat(AnimatorKey.Hash.Speed, speed, _isGround && isMoving ? 0.15f : 0f, Time.deltaTime);
        _animator.SetBool(AnimatorKey.Hash.IsGround, _isGround);

        // 점프 중 하강하는 경우
        bool isFalling = !_isGround && _isJumping && _rigidbody.velocity.y < 0f;
        _animator.SetBool(AnimatorKey.Hash.IsFall, isFalling);

        // 착지
        if (_isGround && _rigidbody.velocity.y <= 0f)
        {
            _isJumping = false;

            _animator.SetBool(AnimatorKey.Hash.IsJump, false);
            _animator.SetBool(AnimatorKey.Hash.IsFall, false);
        }
    }

    #endregion

    #region Ground

    private bool CheckGround()
    {
        float radius = _capsuleCollider.bounds.extents.x * 0.5f;
        Ray ray = new Ray(transform.position + Vector3.up * radius * 2f, Vector3.down);
        if (!Physics.SphereCast(ray, radius, out RaycastHit hit, radius + 0.1f, LayerKey.Mask.Floor))
        {
            return false;
        }

        // 위쪽을 향하는 면만 지면으로 인정
        return hit.normal.y > 0.5f;
    }

    #endregion

    #region Wall

    /// <summary> 충돌체의 법선벡터로 기울기를 이용하여 바닥인지 판단 후 벽인경우에는 true 반환 
    /// </summary>
    private bool IsMovingIntoWall(Collision collision)
    {
        Vector3 moveDir = GetMoveDir();
        if (moveDir.sqrMagnitude < 0.001f)
        {
            return false;
        }

        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 wallNormal = contact.normal;
            // 바닥이나 천장은 벽으로 취급하지 않는다.
            if (Mathf.Abs(wallNormal.y) > 0.5f)
            {
                continue;
            }

            // 이동 방향과 벽을 향하는 방향의 내적을 구한다.
            // 두 벡터가 정규화되어 있으므로 내적값은 두 벡터 사이각의 cos값이다.
            // 임계값 이상이면 벽을 향해 이동 중인 것으로 판단한다.
            // 이동 방향이 벽을 향하는 방향과 60°(Threshold 이내라면 벽에 정면으로 부딪히는 것
            wallNormal.Normalize();
            float dot = Vector3.Dot(moveDir, -wallNormal);
            if (dot >= _wallBlockThreshold)
            {
                return true;
            }
        }

        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isWallBlocked)
        {
            return;
        }

        if (!_isJumping)
        {
            return;
        }

        if (IsMovingIntoWall(collision))
        {
            StartWallFall();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (_isWallBlocked)
        {
            return;
        }

        if (!_isJumping)
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