using System.Collections;
using UnityEngine;

/// <summary> 플레이어를 중심으로 회전, 줌, 충돌 보정 및 셰이킹을 처리하는 카메라 컨트롤러다. </summary>
public class CamCtrl : MonoBehaviour
{
    #region ===== 참조 =====

    private Transform _target;
    private CapsuleCollider _targetCol;

    #endregion ===== 참조 =====

    #region ===== 상태 =====

    private float _eulerY;
    private float _eulerX;

    private Vector3 _targetFocusPos;
    private Vector3 _zoomFocusPos;

    private float _currentTopViewDistance;
    private float _topViewDistanceVelocity;

    private float _currentCollisionDistance;
    private float _collisionDistanceVelocity;

    private float _targetZoomDistance;
    private float _currentZoomDistance;
    private float _zoomVelocity;

    private Vector3 _shakeOffset;
    private bool _isShake;

    #endregion ===== 상태 =====

    #region ===== 카메라 설정 =====

    [Header("카메라 세팅")]
    [SerializeField] private float _horizontalSpeed = 3f;
    [SerializeField] private float _verticalSpeed = 2f;

    [Tooltip("게임 시작 시 카메라의 초기 상하 각도")]
    [SerializeField, Range(-90f, 90f)] private float _initialXAngle = 20f;

    [Tooltip("카메라 위로 보는 각도")]
    [SerializeField] private float _limitUpAngle = 60f;

    [Tooltip("카메라 아래로 보는 각도")]
    [SerializeField] private float _limitDownAngle = 70f;

    [Tooltip("X = 숄더 뷰, Y = 카메라 높이, Z = 기본 카메라 거리")]
    [SerializeField] private Vector3 _camOffset = new(0f, 0f, -4f);

    #endregion ===== 카메라 설정 =====

    #region ===== 카메라 초점 =====

    [Header("카메라 초점")]
    [Tooltip("일반 Orbit 카메라 초점 높이")]
    [SerializeField, Range(0f, 1f)] private float _orbitFocusHeightRatio = 0.65f;

    [Tooltip("Zoom 시 사용하는 카메라 초점 높이")]
    [SerializeField, Range(0f, 1f)] private float _zoomFocusHeightRatio = 0.5f;

    #endregion ===== 카메라 초점 =====

    #region ===== 거리 보정 =====

    [Header("정수리 뷰 보정")]
    [Tooltip("카메라가 위에서 내려다볼수록 추가로 확보할 카메라 거리")]
    [SerializeField] private float _topViewDistBonus = 6f;

    [Tooltip("정수리 뷰 보정이 부드럽게 적용되는 시간")]
    [SerializeField] private float _topViewSmoothTime = 0.08f;

    #endregion ===== 거리 보정 =====

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

    #endregion ===== 카메라 줌 =====

    #region ===== 카메라 셰이킹 =====

    [Header("카메라 셰이킹")]
    [Tooltip("카메라 흔들림이 유지되는 시간")]
    [SerializeField] private float _shakeTime = 0.05f;

    [Tooltip("카메라 흔들림의 최대 거리")]
    [SerializeField] private float _shakeStrength = 0.5f;

    #endregion ===== 카메라 셰이킹 =====

    private void Start()
    {
        if (_target == null)
        {
            CPrint.Error("[CamCtrl] 타겟 세팅이 필요합니다.");
            return;
        }

        InitializeTargetCollider();
        InitializeCameraState();

        UpdateTargetFocusPos();
        UpdateCameraTransform();
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }

        if (Time.timeScale <= 0f)
        {
            return;
        }

        UpdateTargetFocusPos();
        UpdateCameraInput();
        UpdateCameraTransform();
    }

    #region ===== 초기화 =====

    /// <summary> 타겟의 CapsuleCollider 참조를 초기화한다. </summary>
    private void InitializeTargetCollider()
    {
        _targetCol = _target.GetComponent<CapsuleCollider>();
    }

    /// <summary> 카메라의 초기 회전과 거리 상태를 설정한다. </summary>
    private void InitializeCameraState()
    {
        _currentZoomDistance = Mathf.Clamp(Mathf.Abs(_camOffset.z), _minZoomDistance, _maxZoomDistance);
        _targetZoomDistance = _currentZoomDistance;

        _eulerX = Mathf.Clamp(_initialXAngle, -_limitUpAngle, _limitDownAngle);
        _eulerY = _target.eulerAngles.y;

        _currentTopViewDistance = GetTopViewDistance();
        _currentCollisionDistance = _currentZoomDistance + _currentTopViewDistance;
    }

    /// <summary> 카메라 방향을 타겟이 바라보는 방향으로 초기화하고 지정한 줌 거리를 적용한다. </summary>
    public void ResetRotationToTarget(float zoomDistance)
    {
        if (_target == null)
        {
            return;
        }

        _eulerY = _target.eulerAngles.y;
        _eulerX = Mathf.Clamp(_eulerX, -_limitUpAngle, _limitDownAngle);

        float clampedZoomDistance = Mathf.Clamp(zoomDistance, _minZoomDistance, _maxZoomDistance);

        _targetZoomDistance = clampedZoomDistance;
        _currentZoomDistance = clampedZoomDistance;
        _zoomVelocity = 0f;

        UpdateTargetFocusPos();
        UpdateCameraTransform();
    }

    #endregion ===== 초기화 =====

    #region ===== 타겟 =====

    /// <summary> 카메라가 추적할 타겟을 설정하고 관련 참조를 갱신한다. </summary>
    public void SetTarget(Transform target)
    {
        _target = target;
        _targetCol = _target != null ? _target.GetComponent<CapsuleCollider>() : null;
    }

    /// <summary> 타겟 위치와 콜라이더를 기준으로 카메라 초점을 갱신한다. </summary>
    private void UpdateTargetFocusPos()
    {
        if (_targetCol == null)
        {
            CPrint.Warning("[CamCtrl] Target CapsuleCollider를 찾을 수 없습니다.");

            _targetFocusPos = _target.position + Vector3.up * 1.5f;
            _zoomFocusPos = _target.position + Vector3.up;
            return;
        }

        _targetFocusPos = _target.position + Vector3.up * _targetCol.height * _orbitFocusHeightRatio;
        _zoomFocusPos = _target.position + Vector3.up * _targetCol.height * _zoomFocusHeightRatio;
    }

    #endregion ===== 타겟 =====

    #region ===== 입력 =====

    /// <summary> 플레이어 입력에 따라 카메라 회전과 줌을 갱신한다. </summary>
    private void UpdateCameraInput()
    {
        if (!Managers.Input.CanReceivePlayer)
        {
            return;
        }

        _eulerY += Managers.Input.MouseAxisX * _horizontalSpeed;
        _eulerX += Managers.Input.MouseAxisY * _verticalSpeed;

        _eulerX = Mathf.Clamp(_eulerX, -_limitUpAngle, _limitDownAngle);

        UpdateZoom();
    }

    /// <summary> 마우스 휠 입력에 따라 목표 줌 거리를 갱신한다. </summary>
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

    #endregion ===== 입력 =====

    #region ===== 카메라 =====

    /// <summary> 현재 카메라 상태를 기준으로 위치와 회전을 갱신한다. </summary>
    private void UpdateCameraTransform()
    {
        Quaternion aimRotation = Quaternion.Euler(_eulerX, _eulerY, 0f);
        Quaternion camYRotation = Quaternion.Euler(0f, _eulerY, 0f);

        UpdateZoomDistance();
        UpdateTopViewDistance();

        Vector3 cameraPivot = GetCameraPivot();
        Vector3 desiredPosition = GetDesiredCameraPosition(cameraPivot, aimRotation, camYRotation);

        UpdateCollisionDistance(desiredPosition, cameraPivot);

        Vector3 cameraDirection = desiredPosition - cameraPivot;

        if (cameraDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        cameraDirection.Normalize();

        transform.position = cameraPivot + cameraDirection * _currentCollisionDistance + _shakeOffset;
        transform.rotation = aimRotation;
    }

    /// <summary> 현재 줌 거리를 목표 줌 거리까지 부드럽게 이동시킨다. </summary>
    private void UpdateZoomDistance()
    {
        _currentZoomDistance = Mathf.SmoothDamp(_currentZoomDistance, _targetZoomDistance, ref _zoomVelocity,
            _zoomSmoothTime);
    }

    /// <summary> 카메라 상하 각도에 따른 정수리 뷰 거리 보정을 갱신한다. </summary>
    private void UpdateTopViewDistance()
    {
        float targetTopViewDistance = GetTopViewDistance();

        _currentTopViewDistance = Mathf.SmoothDamp(_currentTopViewDistance, targetTopViewDistance,
            ref _topViewDistanceVelocity, _topViewSmoothTime);
    }

    /// <summary> 현재 줌 비율에 맞는 카메라 피벗 위치를 반환한다. </summary>
    private Vector3 GetCameraPivot()
    {
        float zoomRatio = Mathf.InverseLerp(_maxZoomDistance, _minZoomDistance, _currentZoomDistance);
        return Vector3.Lerp(_targetFocusPos, _zoomFocusPos, zoomRatio);
    }

    /// <summary> 카메라 회전과 오프셋을 적용한 목표 위치를 계산한다. </summary>
    private Vector3 GetDesiredCameraPosition(Vector3 cameraPivot, Quaternion aimRotation, Quaternion camYRotation)
    {
        float cameraDistance = _currentZoomDistance + _currentTopViewDistance;

        Vector3 shoulderOffset = camYRotation * (Vector3.right * _camOffset.x);
        Vector3 heightOffset = aimRotation * (Vector3.up * _camOffset.y);
        Vector3 distanceOffset = aimRotation * (Vector3.back * cameraDistance);

        return cameraPivot + shoulderOffset + heightOffset + distanceOffset;
    }

    #endregion ===== 카메라 =====

    #region ===== 거리 보정 =====

    /// <summary> 현재 상하 회전에 따라 정수리 뷰 추가 거리를 계산한다. </summary>
    private float GetTopViewDistance()
    {
        float topViewRatio = Mathf.InverseLerp(0f, _limitDownAngle, Mathf.Max(0f, _eulerX));
        return _topViewDistBonus * topViewRatio;
    }

    /// <summary> 카메라 충돌 상태에 따라 실제 카메라 거리를 갱신한다. </summary>
    private void UpdateCollisionDistance(Vector3 desiredPosition, Vector3 cameraPivot)
    {
        float safeDistance = GetSafeCamDistance(desiredPosition, cameraPivot);

        if (safeDistance < _currentCollisionDistance)
        {
            _currentCollisionDistance = safeDistance;
            _collisionDistanceVelocity = 0f;
            return;
        }

        _currentCollisionDistance = Mathf.SmoothDamp(_currentCollisionDistance, safeDistance,
            ref _collisionDistanceVelocity, _collisionSmoothTime);
    }

    /// <summary> 플레이어와 카메라 사이의 충돌 여부를 확인하고 안전한 카메라 거리를 반환한다. </summary>
    private float GetSafeCamDistance(Vector3 desiredPosition, Vector3 cameraPivot)
    {
        Vector3 pivotToCamera = desiredPosition - cameraPivot;
        float desiredDistance = pivotToCamera.magnitude;

        Debug.DrawLine(cameraPivot, desiredPosition, Color.white);

        if (desiredDistance < 0.1f)
        {
            return 0f;
        }

        Vector3 cameraDirection = pivotToCamera / desiredDistance;

        if (!Physics.SphereCast(cameraPivot, _collisionRadius, cameraDirection, out RaycastHit hit,
                desiredDistance, _collisionCheckLayerMask, QueryTriggerInteraction.Ignore))
        {
            return desiredDistance;
        }

        Debug.DrawLine(cameraPivot, desiredPosition, Color.red);

        float safeDistance = hit.distance - _collisionOffset;
        return Mathf.Max(safeDistance, _minCameraDistance);
    }

    #endregion ===== 거리 보정 =====

    #region ===== 카메라 셰이킹 =====

    /// <summary> 지정한 배율만큼 카메라 셰이킹을 시작한다. </summary>
    public void ShakeCamera(int factor = 1)
    {
        if (_isShake)
        {
            return;
        }

        _isShake = true;

        Vector3 randomOffset = Random.insideUnitSphere;
        randomOffset.z = 0f;

        _shakeOffset = randomOffset * _shakeStrength * factor;

        StartCoroutine(Co_ShakeCamera());
    }

    /// <summary> 지정된 시간 동안 카메라 셰이킹을 유지한 뒤 초기화한다. </summary>
    private IEnumerator Co_ShakeCamera()
    {
        yield return new WaitForSecondsRealtime(_shakeTime);

        _shakeOffset = Vector3.zero;
        _isShake = false;
    }

    #endregion ===== 카메라 셰이킹 =====
}