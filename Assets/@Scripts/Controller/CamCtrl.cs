using System.Collections;
using UnityEngine;

public class CamCtrl : MonoBehaviour
{
    private Transform _target;
    private CapsuleCollider _targetCol;

    // 타겟 초점 좌표
    private Vector3 _targetFocusPos;

    [Header("카메라 세팅")]
    [SerializeField] private float _horizontalSpeed = 3f;
    [SerializeField] private float _verticalSpeed = 3f;
    [Tooltip("카메라 위로 보는 각도")]
    [SerializeField] private float _limitUpAngle = 60f;
    [Tooltip("카메라 아래로 보는 각도")]
    [SerializeField] private float _limitDownAngle = 30f;
    [Tooltip("X = 숄더 뷰, Y = 카메라 높이, Z = 카메라 거리")]
    [SerializeField] private Vector3 _camOffset = new(0f, 0f, -3f);
    
    [Header("카메라 셰이킹")]
    [Tooltip("카메라 흔들림이 유지되는 시간")]
    [SerializeField] private float _shakeTime = 0.05f;
    [Tooltip("카메라 흔들림의 최대 거리")]
    [SerializeField] private float _shakeStrength = 0.5f;
    private Vector3 _shakeOffset;
    private bool _isShake;
    
    [Header("카메라 충돌")]
    [Tooltip("카메라 충돌 체크 레이어")] 
    [SerializeField] private LayerMask _collisionCheckLayerMask;
    [Tooltip("카메라 충돌 검사 반지름")]
    [SerializeField] private float _collisionRadius = 0.2f;
    [Tooltip("충돌했을 때 카메라와 벽 사이에 유지할 거리")]
    [SerializeField] private float _collisionOffset = 0.1f;
    [Tooltip("카메라가 플레이어와 너무 가까워지지 않도록 유지할 최소 거리")]
    [SerializeField] private float _minCameraDistance = 0.5f;
    
    private float _eulerY;
    private float _eulerX;

    private void Start()
    {
        if (_target == null)
        {
            CPrint.Error("타겟 세팅이 필요합니다.");
            return;
        }
        _targetCol = _target.GetComponent<CapsuleCollider>();
        UpdateTargetFocusPos();

        transform.position = _targetFocusPos + _camOffset;
        transform.rotation = Quaternion.identity;

        _eulerX = 0f;
        _eulerY = _target.eulerAngles.y;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void UpdateTargetFocusPos()
    {
        if (_targetCol != null)
        {
            _targetFocusPos = _target.position + Vector3.up * _targetCol.height * 0.8f;
        }
        else
        {
            _targetFocusPos = _target.position + Vector3.up * 1.5f;
        }
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }

        UpdateTargetFocusPos();
        
        if (Managers.Input.CanReceiveInput)
        {
            _eulerY += Managers.Input.MouseAxisX * _horizontalSpeed;
            _eulerX += Managers.Input.MouseAxisY * _verticalSpeed;
        }

        _eulerX = Mathf.Clamp(_eulerX, -_limitUpAngle, _limitDownAngle);

        Quaternion aimRotation = Quaternion.Euler(_eulerX, _eulerY, 0f);
        Quaternion camYRotation = Quaternion.Euler(0f, _eulerY, 0f);

        Vector3 distanceOffset = Vector3.forward * _camOffset.z;
        Vector3 shoulderOffset = Vector3.right * _camOffset.x;
        Vector3 heightOffset = Vector3.up * _camOffset.y;
        Vector3 desiredPos = _targetFocusPos + camYRotation * distanceOffset + shoulderOffset + aimRotation * heightOffset;
        Vector3 safePos = GetSafeCamPos(desiredPos);

        transform.rotation = aimRotation;
        transform.position = safePos + _shakeOffset;
    }

    // 타켓과 카메라 사이에 충돌체가 있는경우 카메라는 타겟과 충돌체사이에 둔다.
    private Vector3 GetSafeCamPos(Vector3 desiredPos)
    {
        Vector3 targetToCamVec = desiredPos - _targetFocusPos;

        float targetToCamDist = targetToCamVec.magnitude;
        if (targetToCamDist < 0.1f)
        {
            return _targetFocusPos;
        }

        Vector3 targetToCamDir = targetToCamVec.normalized;

        if (Physics.SphereCast(_targetFocusPos, _collisionRadius, targetToCamDir, out RaycastHit hit,
                targetToCamDist, _collisionCheckLayerMask))
        {
            float safeDistance = hit.distance - _collisionOffset;

            safeDistance = Mathf.Max(safeDistance, _minCameraDistance);
            
            return _targetFocusPos + targetToCamDir * safeDistance;
        }

        return desiredPos;
    }

    public void ShakeCamera()
    {
        if (!_isShake)
        {
            _isShake = true;
            
            Vector3 randomOffset = Random.insideUnitSphere;
            randomOffset.z = 0f;

            _shakeOffset = randomOffset * _shakeStrength;

            StartCoroutine(Co_ShakeCamera());
        }
    }

    private IEnumerator Co_ShakeCamera()
    {
        yield return new WaitForSecondsRealtime(_shakeTime);

        _shakeOffset = Vector3.zero;
        _isShake = false;
    }
}