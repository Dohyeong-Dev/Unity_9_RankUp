using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 일정 주기로 시야 범위 내의 타겟을 감지하고 가장 가까운 타겟을 관리한다. </summary>
public class FieldOfView : MonoBehaviour
{
    #region ===== 상태 =====

    private readonly List<Transform> _visibleTargets = new();

    private Transform _currentTarget;
    public Transform CurrentTarget => _currentTarget;

    private Coroutine _detectionCoroutine;

    #endregion ===== 상태 =====
    
    #region ===== 설정 =====

    [Header("감지 설정")]
    [Tooltip("타겟을 감지할 수 있는 최대 거리")]
    [SerializeField] private float _detectionRadius = 5f;

    [Tooltip("감지 가능한 시야각")]
    [Range(0f, 360f)]
    [SerializeField] private float _fieldOfViewAngle = 180f;

    [Tooltip("타겟 탐색 주기")]
    [SerializeField] private float _detectionInterval = 0.2f;

    [Tooltip("감지할 타겟 레이어")]
    [SerializeField] private LayerMask _targetLayer;

    [Tooltip("시야를 가리는 장애물 레이어")]
    [SerializeField] private LayerMask _obstacleLayer;

    #endregion ===== 설정 =====

    private void OnEnable()
    {
        StartDetection();
    }

    private void OnDisable()
    {
        StopDetection();
        ClearTarget();
    }

    #region ===== 감지 =====

    /// <summary> 일정 주기로 타겟 감지를 시작한다. </summary>
    private void StartDetection()
    {
        if (_detectionCoroutine != null)
        {
            return;
        }

        _detectionCoroutine = StartCoroutine(Co_UpdateDetection());
    }

    /// <summary> 실행 중인 타겟 감지를 중지한다. </summary>
    public void StopDetection()
    {
        if (_detectionCoroutine == null)
        {
            return;
        }

        StopCoroutine(_detectionCoroutine);
        _detectionCoroutine = null;
    }

    /// <summary> 일정 주기로 시야 범위 내의 타겟 정보를 갱신한다. </summary>
    private IEnumerator Co_UpdateDetection()
    {
        WaitForSeconds wait = new WaitForSeconds(_detectionInterval);

        while (true)
        {
            UpdateVisibleTargets();

            yield return wait;
        }
    }

    /// <summary> 시야 범위 내의 타겟을 탐색하고 가장 가까운 타겟을 현재 타겟으로 설정한다. </summary>
    private void UpdateVisibleTargets()
    {
        ClearTarget();

        Collider[] detectedColliders = Physics.OverlapSphere(transform.position, _detectionRadius, _targetLayer);

        float minimumViewDot = GetMinimumViewDot();
        float closestDistance = float.MaxValue;

        foreach (Collider targetCollider in detectedColliders)
        {
            if (!IsDetectableTarget(targetCollider, out Transform targetTransform))
            {
                continue;
            }

            Vector3 directionToTarget = targetTransform.position - transform.position;
            float distanceToTarget = directionToTarget.magnitude;

            if (distanceToTarget <= Mathf.Epsilon)
            {
                continue;
            }

            directionToTarget /= distanceToTarget;

            if (!IsWithinFieldOfView(directionToTarget, minimumViewDot))
            {
                continue;
            }

            if (IsViewBlocked(directionToTarget, distanceToTarget))
            {
                continue;
            }

            _visibleTargets.Add(targetTransform);

            if (distanceToTarget < closestDistance)
            {
                closestDistance = distanceToTarget;
                _currentTarget = targetTransform;
            }
        }
    }

    /// <summary> 감지 대상이 유효하고 살아있는지 확인한다. </summary>
    private bool IsDetectableTarget(Collider targetCollider, out Transform targetTransform)
    {
        targetTransform = null;

        if (targetCollider == null)
        {
            return false;
        }

        if (targetCollider.TryGetComponent(out IDamageable damageable) && damageable.IsDead)
        {
            return false;
        }

        targetTransform = targetCollider.transform;
        return true;
    }

    /// <summary> 현재 시야각의 최소 내적 값을 계산한다. </summary>
    private float GetMinimumViewDot()
    {
        float halfFieldOfViewAngle = _fieldOfViewAngle * 0.5f;
        return Mathf.Cos(halfFieldOfViewAngle * Mathf.Deg2Rad);
    }

    /// <summary> 지정된 방향이 현재 시야각 안에 있는지 확인한다. </summary>
    private bool IsWithinFieldOfView(Vector3 directionToTarget, float minimumViewDot)
    {
        float viewDot = Vector3.Dot(transform.forward, directionToTarget);
        return viewDot >= minimumViewDot;
    }

    /// <summary> 타겟까지의 시야가 장애물에 의해 가려졌는지 확인한다. </summary>
    private bool IsViewBlocked(Vector3 directionToTarget, float distanceToTarget)
    {
        Vector3 rayOrigin = transform.position + Vector3.up;

        return Physics.Raycast(rayOrigin, directionToTarget, distanceToTarget, _obstacleLayer,
            QueryTriggerInteraction.Ignore);
    }

    /// <summary> 현재 감지 중인 모든 타겟 정보를 초기화한다. </summary>
    public void ClearTarget()
    {
        _currentTarget = null;
        _visibleTargets.Clear();
    }

    #endregion ===== 감지 =====

    #region ===== Gizmos =====

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;

        DrawDetectionRadius(origin);
        DrawFieldOfView(origin);
        DrawVisibleTargets(origin);
    }

    /// <summary> 감지 범위를 표시한다. </summary>
    private void DrawDetectionRadius(Vector3 origin)
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(origin, _detectionRadius);
    }

    /// <summary> 시야각 범위를 표시한다. </summary>
    private void DrawFieldOfView(Vector3 origin)
    {
        Vector3 leftDirection = GetDirectionFromAngle(-_fieldOfViewAngle * 0.5f);
        Vector3 rightDirection = GetDirectionFromAngle(_fieldOfViewAngle * 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + leftDirection * _detectionRadius);
        Gizmos.DrawLine(origin, origin + rightDirection * _detectionRadius);
    }

    /// <summary> 현재 감지 중인 타겟을 표시한다. </summary>
    private void DrawVisibleTargets(Vector3 origin)
    {
        Gizmos.color = Color.red;

        foreach (Transform targetTransform in _visibleTargets)
        {
            if (targetTransform == null)
            {
                continue;
            }

            Gizmos.DrawLine(origin, targetTransform.position);
        }
    }

    /// <summary> 로컬 시야각을 월드 방향 벡터로 변환한다. </summary>
    private Vector3 GetDirectionFromAngle(float angle)
    {
        float worldAngle = angle + transform.eulerAngles.y;
        float angleInRadians = worldAngle * Mathf.Deg2Rad;

        return new Vector3(Mathf.Sin(angleInRadians), 0f, Mathf.Cos(angleInRadians));
    }

    #endregion ===== Gizmos =====
}