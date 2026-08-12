using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CamCtrl : MonoBehaviour
{
    [SerializeField] private Transform _target;

    /// <summary> 타겟 초점 좌표 </summary>
    private Vector3 _targetFocusPos;

    [Header("Camera Settings")]
    [SerializeField] private float _horizontalSpeed = 3f;
    [SerializeField] private float _verticalSpeed = 3f;
    [Tooltip("카메라 위로 보는 각도")]
    [SerializeField] private float _limitUpAngle = 60f;
    [Tooltip("카메라 아래로 보는 각도")]
    [SerializeField] private float _limitDownAngle = 30f;
    [Tooltip("X = 숄더 뷰, Y = 카메라 높이, Z = 카메라 거리")]
    [SerializeField] private Vector3 _camOffset = new Vector3(0f, 0f, -3f);
    [Tooltip("카메라의 보간 이동 속도 [클수록 1에 가까움]")]
    [SerializeField] private float _sharpness = 25f;
    
    [Header("Camera Collision")]
    [Tooltip("카메라 충돌 검사 반지름")]
    [SerializeField] private float _collisionRadius = 0.2f;
    [Tooltip("충돌했을 때 카메라와 벽 사이에 유지할 거리")]
    [SerializeField] private float _collisionOffset = 0.1f;

    // 현재 카메라 수평 / 수직 회전값
    private float _eulerY;
    private float _eulerX;

    private void Start()
    {
        if (_target == null)
        {
            CPrint.Error("타겟 세팅이 필요합니다.");
            return;
        }

        UpdateTargetFocusPos();

        // 초기 카메라 위치
        transform.position = _targetFocusPos + _camOffset;

        // 초기 카메라 회전
        transform.rotation = Quaternion.identity;

        // 초기 수직 회전
        _eulerX = 0f;

        // 플레이어 방향에 카메라 방향을 맞춘다.
        _eulerY = _target.eulerAngles.y;
    }

    private void UpdateTargetFocusPos()
    {
        CapsuleCollider capsule = _target.GetComponent<CapsuleCollider>();

        if (capsule != null)
        {
            _targetFocusPos = _target.position + Vector3.up * capsule.height * 0.8f;
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

        // 마우스 입력
        _eulerY += Managers.Input.MouseAxisX * _horizontalSpeed;
        _eulerX += Managers.Input.MouseAxisY * _verticalSpeed;

        // 위 / 아래 회전 제한
        _eulerX = Mathf.Clamp(_eulerX, -_limitUpAngle, _limitDownAngle);

        // 카메라 회전
        Quaternion aimRotation = Quaternion.Euler(_eulerX, _eulerY, 0f);
        Quaternion camYRotation = Quaternion.Euler(0f, _eulerY, 0f);
        
        // 원래 카메라 위치
        Vector3 distanceOffset = Vector3.forward * _camOffset.z;
        Vector3 shoulderOffset = Vector3.right * _camOffset.x;
        Vector3 heightOffset = Vector3.up * _camOffset.y;
        
        Vector3 desiredPos = _targetFocusPos + camYRotation * shoulderOffset + camYRotation * heightOffset
                             + aimRotation * distanceOffset;

        // 충돌 검사 후 안전한 카메라 위치 계산
        Vector3 safePos = GetSafeCamPos(desiredPos);

        // 최종 카메라 회전
        transform.rotation = aimRotation;

        // 최종 카메라 위치 (보간이동)
        float positionT = 1f - Mathf.Exp(-_sharpness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, safePos, positionT);
    }

    // 타겟과 카메라 사이의 충돌을 검사하고 충돌하면 카메라를 타겟 방향으로 당긴다.
    private Vector3 GetSafeCamPos(Vector3 desiredPos)
    {
        Vector3 targetToCamVec = desiredPos - _targetFocusPos;
        float targetToCamDist = targetToCamVec.magnitude;

        if ( targetToCamDist < 0.1f)
        {
            return _targetFocusPos;
        }

        Vector3 targetToCamDir = targetToCamVec.normalized;

        if (Physics.SphereCast(_targetFocusPos, _collisionRadius, targetToCamDir, out RaycastHit hit,
             targetToCamDist, LayerKey.Mask.Floor))
        {
            float safeDistance = hit.distance - _collisionOffset;
            safeDistance = Mathf.Max(0f, safeDistance);

            return _targetFocusPos + targetToCamDir * safeDistance;
        }

        return desiredPos;
    }
}