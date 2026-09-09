using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary> 컷신 동안 가짜 플레이어의 이동과 회전 애니메이션 연출을 관리한다. </summary>
[RequireComponent(typeof(Animator))]
public class CutscenePlayer : MonoBehaviour
{
    #region ===== 참조 =====

    private Animator _animator;

    #endregion ===== 참조 =====
    
    #region ===== 설정 =====

    [Header("이동")]
    [SerializeField] private float _moveDistance = 6f;
    [SerializeField] private float _moveDuration = 4f;

    [Header("회전 보정")]
    [SerializeField] private float _rotationCorrectionDuration = 0.3f;
    [SerializeField] private Ease _rotationCorrectionEase = Ease.OutSine;

    [Header("연출 시간")]
    [Tooltip("회전을 시작하기까지의 대기 시간")]
    [SerializeField] private float _turnDelay = 3f;
    [Tooltip("회전이 완료된 후 완료 이벤트를 발생시키기까지의 대기 시간")]
    [SerializeField] private float _turnFinishedDelay = 1.5f;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private Coroutine _cutsceneCoroutine;

    private Tween _moveTween;
    private Tween _rotationTween;

    #endregion ===== 상태 =====

    #region ===== 이벤트 =====

    /// <summary> 플레이어가 회전하기 직전에 호출된다. </summary>
    public event Action OnBeforeTurn;

    /// <summary> 플레이어의 회전 연출이 완전히 끝난 후 호출된다. </summary>
    public event Action OnTurnFinished;

    #endregion ===== 이벤트 =====

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnDisable()
    {
        StopCutscene();
    }

    #region ===== 컷신 =====

    /// <summary> 컷신 플레이어 이동과 회전 연출을 시작한다. </summary>
    public void StartCutscene()
    {
        StopCutscene();

        _cutsceneCoroutine = StartCoroutine(Co_StartCutscene());
    }

    /// <summary> 걷기, 회피, 회전 연출을 순서대로 재생한다. </summary>
    private IEnumerator Co_StartCutscene()
    {
        yield return MoveForward();

        OnBeforeTurn?.Invoke();

        yield return new WaitForSeconds(_turnDelay);

        _animator.Play(AnimatorKey.Hash.TurnR);

        yield return new WaitForSeconds(_turnFinishedDelay);

        yield return CorrectRotation();

        _cutsceneCoroutine = null;

        OnTurnFinished?.Invoke();
    }

    /// <summary> 현재 바라보는 방향으로 설정된 거리만큼 이동한다. </summary>
    private IEnumerator MoveForward()
    {
        Vector3 targetPosition = transform.position + transform.forward * _moveDistance;

        _animator.Play(AnimatorKey.Hash.Walk);

        _moveTween = transform.DOMove(targetPosition, _moveDuration).SetEase(Ease.InOutSine);

        yield return _moveTween.WaitForCompletion();

        _moveTween = null;

        _animator.Play(AnimatorKey.Hash.Evade);
    }

    /// <summary> 현재 Y축 회전을 가장 가까운 90도 단위로 보정한다. </summary>
    private IEnumerator CorrectRotation()
    {
        float currentY = transform.eulerAngles.y;
        float targetY = Mathf.Round(currentY / 90f) * 90f;

        Vector3 targetRotation = new Vector3(transform.eulerAngles.x, targetY, transform.eulerAngles.z);

        _rotationTween = transform.DORotate(targetRotation, _rotationCorrectionDuration)
            .SetEase(_rotationCorrectionEase);

        yield return _rotationTween.WaitForCompletion();

        _rotationTween = null;
    }

    /// <summary> 진행 중인 컷신 Coroutine과 Tween을 중지한다. </summary>
    private void StopCutscene()
    {
        if (_cutsceneCoroutine != null)
        {
            StopCoroutine(_cutsceneCoroutine);
            _cutsceneCoroutine = null;
        }

        _moveTween?.Kill();
        _rotationTween?.Kill();

        _moveTween = null;
        _rotationTween = null;
    }
    
    #endregion ===== 컷신 =====
}