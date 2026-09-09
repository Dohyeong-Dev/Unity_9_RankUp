using System;
using System.Collections;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

/// <summary> 보스 등장 컷신에서 Cinemachine 카메라의 타겟 전환과 이동 연출을 관리한다. </summary>
public class CutsceneCamera : MonoBehaviour
{
    #region ===== 설정 =====

    [Header("컷신 가상 카메라")]
    [SerializeField] private CinemachineVirtualCamera _bossEncounterCamera;

    [Header("플레이어 카메라 타겟")]
    [SerializeField] private Transform _playerFollowTarget;
    [SerializeField] private Transform _playerLookAtTarget;

    [Header("보스 카메라 도착 위치")]
    [SerializeField] private Transform _bossFollowTarget;
    [SerializeField] private Transform _bossLookAtTarget;

    [Header("카메라 전환")]
    [SerializeField] private float _bossTargetMoveDuration = 1.5f;
    [SerializeField] private Ease _bossTargetMoveEase = Ease.InOutSine;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private Coroutine _targetMoveCoroutine;

    private Transform _transitionFollowTarget;
    private Transform _transitionLookAtTarget;

    #endregion ===== 상태 =====

    #region ===== 이벤트 =====

    /// <summary> 카메라가 보스 방향의 도착 위치에 도달했을 때 호출된다. </summary>
    public event Action OnBossTargetReached;

    #endregion ===== 이벤트 =====

    private void OnDisable()
    {
        StopTargetMove();
    }

    private void OnDestroy()
    {
        DestroyTransitionTargets();
    }
    
    #region ===== 카메라 이동 =====
    
    /// <summary> 고정된 임시 카메라 타겟을 보스 방향의 도착 위치까지 이동시킨다. </summary>
    public void MoveToBossTarget()
    {
        if (_bossFollowTarget == null || _bossLookAtTarget == null)
        {
            return;
        }

        StopTargetMove();

        if (_transitionFollowTarget == null || _transitionLookAtTarget == null)
        {
            CreateTransitionTargets();
            SetCameraTargetToTransition();
        }

        if (_transitionFollowTarget != null && _transitionLookAtTarget != null)
        {
            _targetMoveCoroutine = StartCoroutine(Co_MoveToBossTarget());
        }
    }

    /// <summary> 임시 카메라 타겟을 보스 방향의 도착 위치까지 동시에 이동시킨다. </summary>
    private IEnumerator Co_MoveToBossTarget()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Join(_transitionFollowTarget.DOMove(_bossFollowTarget.position, _bossTargetMoveDuration)
                .SetEase(_bossTargetMoveEase));
        sequence.Join(_transitionLookAtTarget.DOMove(_bossLookAtTarget.position, _bossTargetMoveDuration)
                .SetEase(_bossTargetMoveEase));

        yield return sequence.WaitForCompletion();

        _targetMoveCoroutine = null;

        OnBossTargetReached?.Invoke();
    }
    
    /// <summary> 진행 중인 카메라 타겟 이동을 중지한다. </summary>
    public void StopTargetMove()
    {
        if (_targetMoveCoroutine == null)
        {
            return;
        }

        StopCoroutine(_targetMoveCoroutine);
        _targetMoveCoroutine = null;
    }
    
    #endregion ===== 카메라 이동 =====
    
    #region ===== 카메라 세팅 =====

    /// <summary> 컷신 카메라의 Follow와 LookAt 타겟을 설정한다. </summary>
    private void SetCameraTarget(Transform followTarget, Transform lookAtTarget)
    {
        if (_bossEncounterCamera == null)
        {
            return;
        }

        _bossEncounterCamera.Follow = followTarget;
        _bossEncounterCamera.LookAt = lookAtTarget;
    }
    
    /// <summary> 컷신 시작 시 카메라가 플레이어 타겟을 따라가도록 설정한다. </summary>
    public void SetCameraTargetToPlayer()
    {
        StopTargetMove();

        SetCameraTarget(_playerFollowTarget, _playerLookAtTarget);
    }
    
    /// <summary> 플레이어 회전 전에 현재 카메라 타겟 위치를 임시 타겟에 복사해 카메라 이동을 고정한다. </summary>
    public void SetCameraTargetToTransition()
    {
        if (_bossEncounterCamera == null)
        {
            return;
        }

        Transform followTarget = _bossEncounterCamera.Follow;
        Transform lookAtTarget = _bossEncounterCamera.LookAt;

        if (followTarget == null || lookAtTarget == null)
        {
            return;
        }

        CreateTransitionTargets();

        _transitionFollowTarget.SetPositionAndRotation(followTarget.position, followTarget.rotation);
        _transitionLookAtTarget.SetPositionAndRotation(lookAtTarget.position, lookAtTarget.rotation);

        SetCameraTarget(_transitionFollowTarget, _transitionLookAtTarget);
    }
    
    #endregion ===== 카메라 세팅 =====

    #region ===== 임시 타겟 관리 =====

    /// <summary> 컷신에서 재사용할 임시 Follow와 LookAt 타겟을 생성한다. </summary>
    private void CreateTransitionTargets()
    {
        if (_transitionFollowTarget == null)
        {
            _transitionFollowTarget = new GameObject("CutsceneTransitionFollowTarget").transform;
            _transitionFollowTarget.SetParent(transform.parent);
        }

        if (_transitionLookAtTarget == null)
        {
            _transitionLookAtTarget = new GameObject("CutsceneTransitionLookAtTarget").transform;
            _transitionLookAtTarget.SetParent(transform.parent);
        }
    }
    
    /// <summary> 생성된 임시 카메라 타겟을 제거한다. </summary>
    private void DestroyTransitionTargets()
    {
        if (_transitionFollowTarget != null)
        {
            _transitionFollowTarget.DOKill();
            Destroy(_transitionFollowTarget.gameObject);
            _transitionFollowTarget = null;
        }

        if (_transitionLookAtTarget != null)
        {
            _transitionLookAtTarget.DOKill();
            Destroy(_transitionLookAtTarget.gameObject);
            _transitionLookAtTarget = null;
        }
    }

    #endregion ===== 임시 타겟 관리 =====
}