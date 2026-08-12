using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCtrl : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotateSpeed = 720f;

    private Rigidbody _rigidbody;
    
    private Camera _camera;
    
    private Vector2 _inputVec;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void SetCamera(Camera cam)
    {
        _camera = cam;
    }
    
    private void Update()
    {
        _inputVec.x = Managers.Input.KeyAxisX;
        _inputVec.y = Managers.Input.KeyAxisY;
    }

    private void FixedUpdate()
    {
        Vector3 moveDir = GetMoveDir();

        Move(moveDir);
        RotateToDir(moveDir);
    }

    private Vector3 GetMoveDir()
    {
        Vector3 cameraForwardVec = _camera.transform.forward;
        cameraForwardVec.y = 0f;
        cameraForwardVec.Normalize();

        Vector3 cameraRightVec = _camera.transform.right;
        cameraRightVec.y = 0f;
        cameraRightVec.Normalize();

        Vector3 moveVec = cameraRightVec * _inputVec.x + cameraForwardVec * _inputVec.y;

        return moveVec.sqrMagnitude > 1f ? moveVec.normalized : moveVec;
    }

    private void Move(Vector3 moveDir)
    {
        Vector3 velocityVec = moveDir * _moveSpeed;
        velocityVec.y = _rigidbody.velocity.y;
        
        _rigidbody.velocity = velocityVec;
    }

    private void RotateToDir(Vector3 dirVec)
    {
        if (dirVec.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(dirVec);

        _rigidbody.MoveRotation(Quaternion.RotateTowards(_rigidbody.rotation, targetRotation,
                _rotateSpeed * Time.fixedDeltaTime));
    }
}