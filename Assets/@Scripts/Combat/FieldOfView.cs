using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    #region -----감지 세팅-----

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

    #endregion -----감지 세팅-----


    #region -----감지 결과-----

    private readonly List<Transform> _visibleTargets = new();

    private Transform _currentTarget;

    #endregion -----감지 결과-----


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

        // 시야 범위에 포함되기 위한 최소 Dot 값
        float minimumViewDot = Mathf.Cos(halfFieldOfViewAngle * Mathf.Deg2Rad);

        float closestDistance = float.MaxValue;

        for (int i = 0; i < detectedColliders.Length; i++)
        {
            Transform targetTransform = detectedColliders[i].transform;

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

    
    #region -----Gizmos-----

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;

        DrawDetectionRadius(origin);
        DrawFieldOfView(origin);
        DrawVisibleTargets(origin);
    }

    /// <summary> 감지 범위를 그린다. </summary>
    private void DrawDetectionRadius(Vector3 origin)
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(origin, _detectionRadius);
    }

    /// <summary> 시야각 경계선을 그린다. </summary>
    private void DrawFieldOfView(Vector3 origin)
    {
        Vector3 leftBoundaryDirection = GetDirectionFromAngle(-_fieldOfViewAngle * 0.5f);
        Vector3 rightBoundaryDirection = GetDirectionFromAngle(_fieldOfViewAngle * 0.5f);

        Gizmos.color = Color.green;

        Gizmos.DrawLine(origin, origin + leftBoundaryDirection * _detectionRadius);
        Gizmos.DrawLine(origin, origin + rightBoundaryDirection * _detectionRadius);
    }

    /// <summary> 현재 감지된 타겟을 표시한다. </summary>
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

    /// <summary> 현재 Transform의 Y축 회전을 기준으로 지정한 각도의 방향 벡터를 반환한다. </summary>
    private Vector3 GetDirectionFromAngle(float angle)
    {
        float worldAngle = angle + transform.eulerAngles.y;
        float angleInRadians = worldAngle * Mathf.Deg2Rad;

        return new Vector3(Mathf.Sin(angleInRadians), 0f, Mathf.Cos(angleInRadians));
    }

    #endregion ------Gizmos-----
}