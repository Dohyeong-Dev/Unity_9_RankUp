using System.Collections;
using UnityEngine;

public class CamCtrl : MonoBehaviour
{
    private Transform _target;
    private CapsuleCollider _targetCol;
    
    private float _eulerY;
    private float _eulerX;
    
    
    #region ===== 카메라 세팅 =====
    
    [Header("카메라 세팅")]

    [SerializeField] private float _horizontalSpeed = 3f;
    [SerializeField] private float _verticalSpeed = 2f;

    [Tooltip("게임 시작 시 카메라의 초기 상하 각도")]
    [SerializeField, Range(-90f, 90f)]
    private float _initialXAngle = 20f;

    [Tooltip("카메라 위로 보는 각도")]
    [SerializeField] private float _limitUpAngle = 60f;

    [Tooltip("카메라 아래로 보는 각도")]
    [SerializeField] private float _limitDownAngle = 70f;

    [Tooltip("X = 숄더 뷰, Y = 카메라 높이, Z = 기본 카메라 거리")]
    [SerializeField] private Vector3 _camOffset = new(0f, 0f, -4f);
    
    #endregion ===== 카메라 세팅 =====
    
    
    #region ===== 카메라 초점 =====
    
    [Header("카메라 초점")]

    private Vector3 _targetFocusPos;
    private Vector3 _zoomFocusPos;
    
    [Tooltip("일반 Orbit 카메라 초점 높이")] // 1이 가까워질수록 플레이어의 콜라이더 위에 가까워진다.
    [SerializeField, Range(0f, 1f)]
    private float _orbitFocusHeightRatio = 0.65f;

    [Tooltip("Zoom 시 사용하는 카메라 초점 높이")] // 1이 가까워질수록 플레이어의 콜라이더 위에 가까워진다.
    [SerializeField, Range(0f, 1f)]
    private float _zoomFocusHeightRatio = 0.5f;
    
    #endregion ===== 카메라 초점 =====

    
    #region ===== 거리 보정 ======
    
    [Header("정수리 뷰 보정")]

    [Tooltip("카메라가 위에서 내려다볼수록 추가로 확보할 카메라 거리")]
    [SerializeField] private float _topViewDistBonus = 6f;

    [Tooltip("정수리 뷰 보정이 부드럽게 적용되는 시간")]
    [SerializeField] private float _topViewSmoothTime = 0.08f;
    private float _topViewDistanceVelocity;

    private float _currentTopViewDistance;

    #endregion ===== 거리 보정 ======

    
    #region ===== 카메라 셰이킹 ======

    [Header("카메라 셰이킹")]

    [Tooltip("카메라 흔들림이 유지되는 시간")]
    [SerializeField] private float _shakeTime = 0.05f;

    [Tooltip("카메라 흔들림의 최대 거리")]
    [SerializeField] private float _shakeStrength = 0.5f;

    private Vector3 _shakeOffset;
    
    private bool _isShake;
    
    #endregion ===== 카메라 셰이킹 ======

    
    #region ===== 카메라 충돌 =====

    [Header("카메라 충돌")]

    [Tooltip("카메라 충돌 체크 레이어")]
    [SerializeField] private LayerMask _collisionCheckLayerMask;

    [Tooltip("카메라 충돌 검사 반지름")]
    [SerializeField] private float _collisionRadius = 0.2f;

    [Tooltip("충돌했을 때 카메라와 벽 사이에 유지할 거리")]
    [SerializeField] private float _collisionOffset = 0.1f;

    [Tooltip("카메라가 플레이어와 너무 가까워지지 않도록 유지할 최소 거리")]
    [SerializeField] private float _minCameraDistance = 0.5f;

    [Tooltip("카메라 충돌 보정이 부드럽게 적용되는 시간")]
    [SerializeField] private float _collisionSmoothTime = 0.08f;

    private float _currentCollisionDistance;
    private float _collisionDistanceVelocity;
    
    #endregion ===== 카메라 충돌 =====
    
    
    #region ===== 카메라 줌 =====

    [Header("카메라 줌")]

    [Tooltip("마우스 휠 줌 속도")]
    [SerializeField] private float _zoomSpeed = 2f;

    [Tooltip("카메라 최소 거리")]
    [SerializeField] private float _minZoomDistance = 1.5f;

    [Tooltip("카메라 최대 거리")]
    [SerializeField] private float _maxZoomDistance = 6f;

    [Tooltip("줌이 부드럽게 따라오는 시간")]
    [SerializeField] private float _zoomSmoothTime = 0.08f;
    private float _zoomVelocity;

    private float _targetZoomDistance;
    private float _currentZoomDistance;

    #endregion ===== 카메라 줌 =====
    

    private void Start()
    {
        if (_target == null)
        {
            CPrint.Error("타겟 세팅이 필요합니다.");
            return;
        }

        _targetCol = _target.GetComponent<CapsuleCollider>();
        UpdateTargetFocusPos();

        // 기본 줌 거리
        _currentZoomDistance = Mathf.Abs(_camOffset.z);
        _currentZoomDistance = Mathf.Clamp(_currentZoomDistance, _minZoomDistance, _maxZoomDistance);
        _targetZoomDistance = _currentZoomDistance;

        // 초기 회전
        _eulerX = Mathf.Clamp(_initialXAngle, -_limitUpAngle, _limitDownAngle);
        _eulerY = _target.eulerAngles.y;

        // 초기 정수리 뷰 거리 계산
        // 시작 각도가 이미 위쪽을 보고 있다면 그 각도에 해당하는 TopView 거리도 처음부터 즉시 적용한다.
        _currentTopViewDistance = GetTopViewDistance();
        
        // 초기 카메라 거리
        _currentCollisionDistance = _currentZoomDistance + _currentTopViewDistance;
        
        // 시작 시 트랜스폼 즉시 적용
        UpdateCameraTransform();
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

            UpdateZoom();
        }

        // 상하 회전 제한
        _eulerX = Mathf.Clamp(_eulerX, -_limitUpAngle, _limitDownAngle);

        UpdateCameraTransform();
    }

    private void UpdateZoom()
    {
        float wheel = Managers.Input.MouseWheel;

        if (Mathf.Approximately(wheel, 0f))
        {
            return;
        }

        _targetZoomDistance -= wheel * _zoomSpeed;
        _targetZoomDistance = Mathf.Clamp(_targetZoomDistance, _minZoomDistance, _maxZoomDistance);
    }
    
    public void SetTarget(Transform target)
    {
        _target = target;
        _targetCol = _target != null ? _target.GetComponent<CapsuleCollider>() : null;
    }

    /// <summary> 카메라 초점(타겟/줌) 계산 </summary>
    private void UpdateTargetFocusPos()
    {
        if (_targetCol != null)
        {
            _targetFocusPos = _target.position + Vector3.up * _targetCol.height * _orbitFocusHeightRatio;
            _zoomFocusPos = _target.position + Vector3.up * _targetCol.height * _zoomFocusHeightRatio;
        }
        else
        {
            CPrint.Warning("No target collider found.");
            _targetFocusPos = _target.position + Vector3.up * 1.5f;
            _zoomFocusPos = _target.position + Vector3.up * 1.0f;
        }
    }

    /// <summary> 정수리 뷰 거리 보정 </summary>
    private float GetTopViewDistance()
    {
        float topViewRatio = Mathf.InverseLerp(0f, _limitDownAngle, Mathf.Max(0f, _eulerX));
        
        return _topViewDistBonus * topViewRatio;
    }
    
    /// <summary> 카메라 위치/회전 업데이트 </summary>
    private void UpdateCameraTransform()
    {
        // 카메라 회전
        Quaternion aimRotation = Quaternion.Euler(_eulerX, _eulerY, 0f);
        Quaternion camYRotation = Quaternion.Euler(0f, _eulerY, 0f);

        // 줌 거리 보간
        _currentZoomDistance = Mathf.SmoothDamp(_currentZoomDistance, _targetZoomDistance, ref _zoomVelocity,
            _zoomSmoothTime);
        
        // 현재 줌 거리에 따라 타겟초점과 줌초점 사이의 피봇을 구한다. 
        float zoomRatio = Mathf.InverseLerp(_maxZoomDistance, _minZoomDistance, _currentZoomDistance);
        Vector3 cameraPivot = Vector3.Lerp(_targetFocusPos, _zoomFocusPos, zoomRatio);

        float targetTopViewDist = GetTopViewDistance();

        // 게임 중에는 부드럽게 이동
        _currentTopViewDistance = Mathf.SmoothDamp(_currentTopViewDistance, targetTopViewDist,
                ref  _topViewDistanceVelocity, _topViewSmoothTime);

        // 실제 카메라 거리
        float cameraDist = _currentZoomDistance + _currentTopViewDistance;

        // 숄더 위치
        Vector3 shoulderOffset = camYRotation * (Vector3.right * _camOffset.x);
        // 카메라 높이 Offset
        Vector3 heightOffset = aimRotation * (Vector3.up * _camOffset.y);
        // 카메라 거리 방향
        Vector3 distOffset = aimRotation * (Vector3.back * cameraDist);

        // 최종 위치
        Vector3 desiredPos = cameraPivot + shoulderOffset + heightOffset + distOffset;

        // 충돌 체크 및 보간 이동
        float safeDistance = GetSafeCamDistance(desiredPos, cameraPivot);

        if (safeDistance < _currentCollisionDistance)
        {
            // 벽에 가까워지는 경우에는 즉시 당긴다.
            _currentCollisionDistance = safeDistance;
            _collisionDistanceVelocity = 0f;
        }
        else
        {
            // 벽에서 멀어지는 경우에만 부드럽게 복귀한다.
            _currentCollisionDistance = Mathf.SmoothDamp(_currentCollisionDistance, safeDistance,
                ref _collisionDistanceVelocity, _collisionSmoothTime);
        }

        Vector3 cameraDirection = (desiredPos - cameraPivot).normalized;
        Vector3 safePos = cameraPivot + cameraDirection * _currentCollisionDistance;

        transform.position = safePos + _shakeOffset;
        transform.rotation = aimRotation;
    }

    /// <summary> 플레이어와 카메라 사이 오브젝트 충돌 시 안전 거리를 구한다 </summary>
    private float GetSafeCamDistance(Vector3 desiredPos, Vector3 pivot)
    {
        Vector3 targetToCamVec = desiredPos - pivot;
        float targetToCamDist = targetToCamVec.magnitude;
        Debug.DrawLine(pivot, desiredPos, Color.white);
        if (targetToCamDist < 0.1f)
        {
            return 0f;
        }

        Vector3 targetToCamDir = targetToCamVec.normalized;
        
        if (Physics.SphereCast(pivot, _collisionRadius, targetToCamDir, out RaycastHit hit,
                targetToCamDist, _collisionCheckLayerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(pivot, desiredPos, Color.red);
            float safeDistance = hit.distance - _collisionOffset;
            return Mathf.Max(safeDistance, _minCameraDistance);
        }

        return targetToCamDist;
    }

    public void ShakeCamera()
    {
        if (_isShake)
        {
            return;
        }

        _isShake = true;

        Vector3 randomOffset = Random.insideUnitSphere;
        randomOffset.z = 0f;

        _shakeOffset = randomOffset * _shakeStrength;

        StartCoroutine(Co_ShakeCamera());
    }

    private IEnumerator Co_ShakeCamera()
    {
        yield return new WaitForSecondsRealtime(_shakeTime);

        _shakeOffset = Vector3.zero;

        _isShake = false;
    }
}