using System.Collections;
using UnityEngine;

public class CamCtrl : MonoBehaviour
{
    [SerializeField] private Transform _target;
    /// <summary> 타켓 초점 좌표 </summary>
    private Vector3 _targetFocusPos;

    [Header("Camera Settings")]
    [SerializeField] private float _horizontalSpeed = 3f;
    [SerializeField] private float _verticalSpeed = 3f;
    [Tooltip("카메라 위로 보는 각도 제한")]
    [SerializeField] private float _limitUpAngle = 60f;
    [Tooltip("카메라 아래로 보는 각도 제한")]
    [SerializeField] private float _limitDownAngle = 30f;
    [Tooltip("X = 숄더 뷰, Y = 카메라 높이, Z = 카메라 거리")]
    [SerializeField] private Vector3 _camOffset = new Vector3(0f, 5f, -7f);

    // 현재 카메라 수평 / 수직 회전값
    private float _eulerY;
    private float _eulerX;

    private void Start()
    {
        if (_target == null)
        {
            CPrint.Error("타켓 세팅이 필요합니다.");
            return;
        }
        
        UpdateTargetFocusPos();

        // 포즈 초기화
        transform.position = _targetFocusPos + _camOffset;
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

        // InputManager의 마우스 입력 사용
        _eulerY += Managers.Input.MouseAxisX * _horizontalSpeed;
        _eulerX += Managers.Input.MouseAxisY * _verticalSpeed;

        // 위 / 아래 회전 제한
        _eulerX = Mathf.Clamp(_eulerX, -_limitUpAngle, _limitDownAngle);
        
        Quaternion aimRotation = Quaternion.Euler(_eulerX, _eulerY, 0f);

        // 최종 카메라 회전
        transform.rotation = aimRotation;

        // 최종 카메라 위치
        transform.position = _targetFocusPos + aimRotation * _camOffset;
    }
}