using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> Popup UI의 입력, 열기 및 닫기 애니메이션을 제공하는 기본 클래스다. </summary>
public abstract class BasePopup : BaseUI
{
    private GraphicRaycaster _graphicRaycaster;
    
    private Action _closeAction;
    
    #region ===== 애니메이션 =====
    
    public enum AnimationType
    {
        None,
        ContentsUp,
        BgUp,
        BgDown,
    }

    public AnimationType CurrentAnimation;

    [Header("ContentsUp 애니메이션")]
    [Tooltip("아래에서 위로 올라가는 거리")]
    [SerializeField] private float _contentsUpOffsetY = 10f;
    
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
        
        _graphicRaycaster = gameObject.GetOrAddComponent<GraphicRaycaster>();

        Managers.UI.SetupCanvas(this);

        PlayAnimation(true);
    }

    protected abstract void OnAwake();
    
    private void Start()
    {
        OnStart();
    }

    protected abstract void OnStart();
    
    private void Update()
    {
        OnUpdate();
    }

    protected abstract void OnUpdate();
    
    /// <summary> Popup에서 처리할 입력을 확인한다. </summary>
    public virtual void OnInputKey()
    {
        if (!Managers.Input.KeyDown_Esc)
        {
            return;
        }

        Close();
    }

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

    /// <summary> Popup 열기 애니메이션이 완료된 후 호출한다. </summary>
    protected virtual void OnOpened()
    {
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

            case AnimationType.ContentsUp:
                PlayContentsUpAnimation(isOpen);
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
            background.transform.DOScale(1f, 0.25f).From(0f).OnComplete(OnOpened);
        }
        else
        {
            background.transform.DOScale(0f, 0.25f).OnComplete(CloseImmediately);
        }
    }

    /// <summary> Popup 콘텐츠가 위로 이동하며 표시되는 애니메이션을 재생한다. </summary>
    private void PlayContentsUpAnimation(bool isOpen)
    {
        TMP_Text contentText = gameObject.FindChild<TMP_Text>("ContentTxt", true);

        if (contentText == null)
        {
            CPrint.Error("Popup ContentText를 찾을 수 없습니다.");
            return;
        }

        if (isOpen)
        {
            PlayContentsUpOpenAnimation(contentText);
            return;
        }

        PlayContentsUpCloseAnimation(contentText);
    }

    /// <summary> 콘텐츠가 위로 이동하며 열리는 애니메이션을 재생한다. </summary>
    private void PlayContentsUpOpenAnimation(TMP_Text contentText)
    {
        contentText.transform.DOLocalMove(Vector3.up * _contentsUpOffsetY, 0.5f).SetRelative(true)
            .From(contentText.transform.localPosition + Vector3.down * _contentsUpOffsetY).OnStart(() =>
            {
                contentText.DOFade(1f, 0.5f)
                    .From(0f)
                    .SetEase(Ease.InOutCirc);
            }).OnComplete(OnOpened);
    }

    /// <summary> 콘텐츠가 아래로 이동하며 닫히는 애니메이션을 재생한다. </summary>
    private void PlayContentsUpCloseAnimation(TMP_Text contentText)
    {
        contentText.transform.DOLocalMove(Vector3.down * _contentsUpOffsetY, 0.25f).SetRelative(true)
            .SetEase(Ease.InOutCirc).OnStart(() =>
            {
                contentText.DOFade(0f, 0.25f)
                    .SetEase(Ease.InOutCirc);
            }).OnComplete(CloseImmediately);
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
        backgroundTransform.DOLocalMove(Vector3.up * _bgUpOffsetY, 0.2f).SetEase(Ease.Linear)
            .SetRelative(true).From(backgroundTransform.localPosition + Vector3.down * _contentsUpOffsetY)
            .OnStart(() =>
            {
                backgroundCanvasGroup.DOFade(0.98f, 0.13f).From(0f);
            }).OnComplete(OnOpened);
    }

    /// <summary> 배경이 아래로 이동하며 닫히는 애니메이션을 재생한다. </summary>
    private void PlayBgUpCloseAnimation(Transform backgroundTransform, CanvasGroup backgroundCanvasGroup)
    {
        backgroundTransform.DOLocalMove(Vector3.down * _bgUpOffsetY, 0.15f).SetEase(Ease.Linear)
            .SetRelative(true).OnStart(() =>
            {
                backgroundCanvasGroup.DOFade(0f, 0.15f).SetEase(Ease.Linear);
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
            .OnStart(() =>
            {
                backgroundCanvasGroup.DOFade(0.98f, 0.2f).From(0f);
            }).OnComplete(OnOpened);
    }

    /// <summary> 배경이 아래로 이동하며 닫히는 애니메이션을 재생한다. </summary>
    private void PlayBgDownCloseAnimation(Transform backgroundTransform)
    {
        Vector3 endPosition = backgroundTransform.localPosition + Vector3.down * GetBgDownDistance();

        backgroundTransform.DOLocalMove(endPosition, 0.3f).SetEase(Ease.InCubic).OnComplete(CloseImmediately);
    }

    #endregion ===== 애니메이션 =====
}