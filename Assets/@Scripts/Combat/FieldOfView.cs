using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    #region ===== 감지 세팅 =====

    [Header("감지 세팅")]
    [Tooltip("타겟을 감지할 수 있는 최대 거리")]
    [SerializeField]
    private float _detectionRadius = 5f;

    [Tooltip("감지 가능한 시야각")]
    [SerializeField, Range(0f, 360f)]
    private float _fieldOfViewAngle = 180f;

    [Tooltip("타겟 탐색 주기")]
    [SerializeField]
    private float _detectionInterval = 0.2f;

    [Tooltip("감지할 타겟 레이어")]
    [SerializeField]
    private LayerMask _targetLayer;

    [Tooltip("시야를 가리는 장애물 레이어")]
    [SerializeField]
    private LayerMask _obstacleLayer;

    #endregion


    #region ===== 감지 결과 =====

    private readonly List<Transform> _visibleTargets = new();

    private Transform _currentTarget;
    public Transform CurrentTarget => _currentTarget;

    #endregion


    private void Start()
    {
        StartCoroutine(Co_UpdateDetection());
    }

    /// <summary> 일정 주기로 타겟 감지 정보를 갱신한다. </summary>
    private IEnumerator Co_UpdateDetection()
    {
        WaitForSeconds wait = new WaitForSeconds(_detectionInterval);

        while (true)
        {
            UpdateVisibleTargets();

            yield return wait;
        }
    }

    /// <summary> 시야 범위 내의 타겟을 탐색하고 현재 타겟을 갱신한다. </summary>
    private void UpdateVisibleTargets()
    {
        _visibleTargets.Clear();
        _currentTarget = null;

        Collider[] detectedColliders = Physics.OverlapSphere(transform.position, _detectionRadius, _targetLayer);

        float halfFieldOfViewAngle = _fieldOfViewAngle * 0.5f;

        float minimumViewDot = Mathf.Cos(halfFieldOfViewAngle * Mathf.Deg2Rad);

        float closestDistance = float.MaxValue;

        for (int i = 0; i < detectedColliders.Length; i++)
        {
            Collider targetCollider = detectedColliders[i];

            // 죽은 플레이어는 감지 대상에서 제외
            if (targetCollider.TryGetComponent(out IDamageable actor))
            {
                if (actor.IsDead)
                {
                    continue;
                }
            }

            Transform targetTransform = targetCollider.transform;

            Vector3 directionToTarget = targetTransform.position - transform.position;

            float distanceToTarget = directionToTarget.magnitude;

            if (distanceToTarget <= Mathf.Epsilon)
            {
                continue;
            }

            directionToTarget /= distanceToTarget;

            // 1. 시야각 판정
            float viewDot = Vector3.Dot(transform.forward, directionToTarget);

            if (viewDot < minimumViewDot)
            {
                continue;
            }

            // 2. 장애물 판정
            Vector3 rayOrigin = transform.position + Vector3.up;

            bool isBlocked = Physics.Raycast(rayOrigin, directionToTarget, distanceToTarget, _obstacleLayer);

            if (isBlocked)
            {
                continue;
            }

            // 3. 감지된 타겟 등록
            _visibleTargets.Add(targetTransform);

            // 가장 가까운 타겟을 현재 타겟으로 설정
            if (distanceToTarget < closestDistance)
            {
                closestDistance = distanceToTarget;
                _currentTarget = targetTransform;
            }
        }
    }

    /// <summary> 현재 감지 중인 타겟을 즉시 제거한다. </summary>
    public void ClearTarget()
    {
        _currentTarget = null;
        _visibleTargets.Clear();
    }

    
    #region -----Gizmos-----

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;

        DrawDetectionRadius(origin);
        DrawFieldOfView(origin);
        DrawVisibleTargets(origin);
    }

    private void DrawDetectionRadius(Vector3 origin)
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(origin, _detectionRadius);
    }

    private void DrawFieldOfView(Vector3 origin)
    {
        Vector3 leftBoundaryDirection = GetDirectionFromAngle(-_fieldOfViewAngle * 0.5f);
        Vector3 rightBoundaryDirection = GetDirectionFromAngle(_fieldOfViewAngle * 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + leftBoundaryDirection * _detectionRadius);
        Gizmos.DrawLine(origin, origin + rightBoundaryDirection * _detectionRadius);
    }

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

    private Vector3 GetDirectionFromAngle(float angle)
    {
        float worldAngle = angle + transform.eulerAngles.y;
        float angleInRadians = worldAngle * Mathf.Deg2Rad;

        return new Vector3(Mathf.Sin(angleInRadians), 0f, Mathf.Cos(angleInRadians));
    }

    #endregion
}