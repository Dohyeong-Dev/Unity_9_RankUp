using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary> Popup UI의 입력, 열기 및 닫기 애니메이션을 제공하는 기본 클래스다. </summary>
[RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
public abstract class BasePopup : BaseUI
{
    protected GraphicRaycaster _graphicRaycaster;
    
    private Action _closeAction;

    public bool IsRaycastEnabled => _graphicRaycaster != null && _graphicRaycaster.isActiveAndEnabled;
    
    #region ===== 애니메이션 =====
    
    public enum AnimationType
    {
        None,
        BgUp,
        BgDown,
    }

    public AnimationType CurrentAnimation;

    [Header("BgUp 애니메이션")]
    [Tooltip("아래에서 위로 올라가는 거리")]
    [SerializeField] private float _bgUpOffsetY = 10f;
    
    [Header("BgDown 애니메이션")]
    [Tooltip("Bg가 위에서 내려오는 거리, 0이면 [DefaultBgDownDistance]을 사용")]
    [Min(0f)]
    [SerializeField] private float _bgDownDistance;

    private const float DefaultBgDownDistance = 300f;

    #endregion ===== 애니메이션 =====

    private void Awake()
    {
        OnAwake();

        Image bg = gameObject.FindChild<Image>("Bg");
        if (bg == null)
        {
            CPrint.Error("[BasePopup] Bg이 존재하지 않습니다.");
        }
        else
        {
            _graphicRaycaster = bg.gameObject.GetOrAddComponent<GraphicRaycaster>();
            _graphicRaycaster.enabled = false;
        }

        Managers.UI.SetupCanvas(this);

        PlayAnimation(true);
    }

    protected abstract void OnAwake();
    
    private void Start()
    {
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimePopup);
        
        OnStart();
    }

    protected abstract void OnStart();

    /// <summary> Popup 열기 애니메이션이 완료된 후 호출한다. </summary>
    protected virtual void OnOpened()
    {
        _graphicRaycaster.enabled = true;
    }
    
    private void Update()
    {
        OnUpdate();
    }

    protected abstract void OnUpdate();

    /// <summary> Popup에서 처리할 입력을 확인한다. </summary>
    public abstract void OnInputKey();

    protected virtual void OnDestroy()
    {
        transform.DOKill(true);
        DOTween.Kill(gameObject);

        DestroyOverride();
    }

    protected abstract void DestroyOverride();

    /// <summary> Popup을 닫고 닫기 완료 후 전달받은 콜백을 실행한다. </summary>
    public virtual void Close(Action closeAction = null)
    {
        _graphicRaycaster.enabled = false;
            
        _closeAction = closeAction;

        PlayAnimation(false);
    }

    /// <summary> Popup의 배경 Transform을 찾는다. </summary>
    private bool TryGetBackground(out Transform backgroundTransform)
    {
        backgroundTransform = gameObject.FindChild<Transform>("Bg");

        if (backgroundTransform != null)
        {
            return true;
        }

        CPrint.Error("Popup Background를 찾을 수 없습니다.");
        return false;
    }

    /// <summary> Popup을 즉시 제거하고 닫기 완료 콜백을 실행한다. </summary>
    private void CloseImmediately()
    {
        Action closeAction = _closeAction;
        _closeAction = null;

        Managers.UI.ClosePopupUI(this);

        closeAction?.Invoke();
    }
    
    #region ===== 애니메이션 =====
    
    /// <summary> Popup 열기 또는 닫기 애니메이션을 재생한다. </summary>
    protected virtual void PlayAnimation(bool isOpen)
    {
        switch (CurrentAnimation)
        {
            case AnimationType.None:
                PlayDefaultAnimation(isOpen);
                break;

            case AnimationType.BgUp:
                PlayBgUpAnimation(isOpen);
                break;

            case AnimationType.BgDown:
                PlayBgDownAnimation(isOpen);
                break;
        }
    }

    /// <summary> 애니메이션이 없는 기본 Popup 열기 및 닫기 애니메이션을 재생한다. </summary>
    private void PlayDefaultAnimation(bool isOpen)
    {
        Image background = gameObject.FindChild<Image>("Bg");

        if (background == null)
        {
            CPrint.Error("Popup Background를 찾을 수 없습니다.");
            return;
        }

        if (isOpen)
        {
            background.transform.DOScale(1f, 0.25f).From(0f).SetUpdate(true).OnComplete(OnOpened);
        }
        else
        {
            background.transform.DOScale(0f, 0.25f).SetUpdate(true).OnComplete(CloseImmediately);
        }
    }

    /// <summary> 배경이 아래에서 위로 이동하는 Popup 애니메이션을 재생한다. </summary>
    private void PlayBgUpAnimation(bool isOpen)
    {
        if (!TryGetBackground(out Transform backgroundTransform))
        {
            return;
        }

        CanvasGroup backgroundCanvasGroup = backgroundTransform.gameObject.GetOrAddComponent<CanvasGroup>();

        if (isOpen)
        {
            PlayBgUpOpenAnimation(backgroundTransform, backgroundCanvasGroup);
            return;
        }

        PlayBgUpCloseAnimation(backgroundTransform, backgroundCanvasGroup);
    }

    /// <summary> 배경이 위로 이동하며 열리는 애니메이션을 재생한다. </summary>
    private void PlayBgUpOpenAnimation(Transform backgroundTransform, CanvasGroup backgroundCanvasGroup)
    {
        backgroundTransform.DOLocalMove(Vector3.up * _bgUpOffsetY, 0.2f).SetEase(Ease.Linear).SetUpdate(true)
            .SetRelative(true).From(backgroundTransform.localPosition + Vector3.down * _bgUpOffsetY)
            .OnStart(() =>
            {
                backgroundCanvasGroup.DOFade(0.98f, 0.13f).From(0f).SetUpdate(true);
            }).OnComplete(OnOpened);
    }

    /// <summary> 배경이 아래로 이동하며 닫히는 애니메이션을 재생한다. </summary>
    private void PlayBgUpCloseAnimation(Transform backgroundTransform, CanvasGroup backgroundCanvasGroup)
    {
        backgroundTransform.DOLocalMove(Vector3.down * _bgUpOffsetY, 0.15f).SetEase(Ease.Linear).SetUpdate(true)
            .SetRelative(true).OnStart(() =>
            {
                backgroundCanvasGroup.DOFade(0f, 0.15f).SetEase(Ease.Linear).SetUpdate(true);
            }).OnComplete(CloseImmediately);
    }

    /// <summary> 배경이 위에서 아래로 이동하는 Popup 애니메이션을 재생한다. </summary>
    private void PlayBgDownAnimation(bool isOpen)
    {
        if (!TryGetBackground(out Transform backgroundTransform))
        {
            return;
        }

        CanvasGroup backgroundCanvasGroup = backgroundTransform.gameObject.GetOrAddComponent<CanvasGroup>();

        if (isOpen)
        {
            PlayBgDownOpenAnimation(backgroundTransform, backgroundCanvasGroup);
            return;
        }

        PlayBgDownCloseAnimation(backgroundTransform);
    }

    /// <summary> BgDown 애니메이션에 사용할 이동 거리를 반환한다. </summary>
    private float GetBgDownDistance()
    {
        return Mathf.Approximately(_bgDownDistance, 0f) ? DefaultBgDownDistance : _bgDownDistance;
    }
    
    /// <summary> 배경이 위에서 내려오며 열리는 애니메이션을 재생한다. </summary>
    private void PlayBgDownOpenAnimation(Transform backgroundTransform, CanvasGroup backgroundCanvasGroup)
    {
        Vector3 targetPosition = backgroundTransform.localPosition;
        Vector3 startPosition = targetPosition + Vector3.up * GetBgDownDistance();

        backgroundTransform.DOLocalMove(targetPosition, 0.35f).SetEase(Ease.OutCubic).From(startPosition)
            .SetUpdate(true).OnStart(() =>
            {
                backgroundCanvasGroup.DOFade(0.98f, 0.2f).From(0f).SetUpdate(true);
            }).OnComplete(OnOpened);
    }

    /// <summary> 배경이 살짝 아래로 갔다가 목표 지점으로 간 후 닫히는 애니메이션을 재생한다. </summary>
    private void PlayBgDownCloseAnimation(Transform backgroundTransform)
    {
        Vector3 endPosition = backgroundTransform.localPosition + Vector3.up * GetBgDownDistance();

        backgroundTransform.DOLocalMove(endPosition, 0.3f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(CloseImmediately);
    }

    #endregion ===== 애니메이션 =====
}