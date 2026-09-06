using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class BasePopup : BaseUI
{
    protected bool IsClosing;

    private Action _closeAction;

    public enum AnimationType
    {
        None,
        ContentsUp, // 컨텐츠가 살짝 올라옴
        BgUp, // 배경이 아래에서 올라옴
        BgDown, // 배경이 위에서 내려옴
    }

    public AnimationType CurrentAnimation;


    [Header("BgDown 애니메이션")]
    [Tooltip("Bg가 위에서 내려오는 거리. 0이면 기본값을 사용합니다.")]
    [Min(0f)]
    [SerializeField] private float _bgDownDistance = 0f;

    private const float DefaultBgDownDistance = 300f;


    private void Awake()
    {
        OnAwake();

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

    public virtual void OnInputKey()
    {
        if (Managers.Input.KeyDown_Esc)
        {
            Close();
        }
    }

    // 팝업 열기/닫기 애니메이션
    // 닫기 애니메이션이 있는 경우 애니메이션 완료 후 팝업을 제거한다.
    protected virtual void PlayAnimation(bool isOpen)
    {
        const float offsetY = 10f;

        switch (CurrentAnimation)
        {
            case AnimationType.None:
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

                break;
            }

            case AnimationType.ContentsUp:
            {
                TMP_Text contentText = gameObject.FindChild<TMP_Text>("ContentTxt", true);

                if (contentText == null)
                {
                    CPrint.Error("Popup ContentText를 찾을 수 없습니다.");
                    return;
                }

                if (isOpen)
                {
                    contentText.transform.DOLocalMove(Vector3.up * offsetY, 0.5f).SetRelative(true)
                        .From(contentText.transform.localPosition + Vector3.down * offsetY).OnStart(() =>
                        {
                            contentText.DOFade(1f, 0.5f).From(0f).SetEase(Ease.InOutCirc);
                        }).OnComplete(OnOpened);
                }
                else
                {
                    contentText.transform.DOLocalMove(Vector3.down * offsetY, 0.25f).SetRelative(true)
                        .SetEase(Ease.InOutCirc).OnStart(() =>
                        {
                            contentText.DOFade(0f, 0.25f).SetEase(Ease.InOutCirc);
                        })
                        .OnComplete(CloseImmediately);
                }

                break;
            }

            case AnimationType.BgUp:
            {
                Transform backgroundTransform = gameObject.FindChild<Transform>("Bg");

                if (backgroundTransform == null)
                {
                    CPrint.Error("Popup Background를 찾을 수 없습니다.");
                    return;
                }

                CanvasGroup backgroundCanvasGroup = backgroundTransform.gameObject.GetOrAddComponent<CanvasGroup>();

                if (isOpen)
                {
                    backgroundTransform.DOLocalMove(Vector3.up * offsetY, 0.2f).SetEase(Ease.Linear)
                        .SetRelative(true).From(backgroundTransform.localPosition + Vector3.down * offsetY)
                        .OnStart(() => { backgroundCanvasGroup.DOFade(0.98f, 0.13f).From(0f); }).OnComplete(OnOpened);
                }
                else

                {
                    backgroundTransform.DOLocalMove(Vector3.down * offsetY, 0.15f).SetEase(Ease.Linear)
                        .SetRelative(true).OnStart(() =>
                        {
                            backgroundCanvasGroup.DOFade(0f, 0.15f).SetEase(Ease.Linear);
                        }).OnComplete(CloseImmediately);
                }

                break;
            }

            case AnimationType.BgDown:
            {
                Transform backgroundTransform = gameObject.FindChild<Transform>("Bg");

                if (backgroundTransform == null)
                {
                    CPrint.Error("Popup Background를 찾을 수 없습니다.");
                    return;
                }

                CanvasGroup backgroundCanvasGroup = backgroundTransform.gameObject.GetOrAddComponent<CanvasGroup>();

                // 인스펙터 값이 0이면 기본 거리 사용
                float downDistance = _bgDownDistance;

                if (Mathf.Approximately(downDistance, 0f))
                {
                    downDistance = DefaultBgDownDistance;
                }

                // 현재 프리팹에 설정되어 있는 위치를 애니메이션의 최종 위치로 사용한다.
                Vector3 targetPosition = backgroundTransform.localPosition;

                if (isOpen)
                {
                    // 최종 위치보다 위에서 시작
                    Vector3 startPosition = targetPosition + Vector3.up * downDistance;

                    backgroundTransform.DOLocalMove(targetPosition, 0.35f).SetEase(Ease.OutCubic)
                        .From(startPosition).OnStart(() => { backgroundCanvasGroup.DOFade(0.98f, 0.2f).From(0f); })
                        .OnComplete(OnOpened);
                }
                else
                {
                    // 닫을 때는 아래로 내려간다.
                    Vector3 endPosition = targetPosition + Vector3.down * downDistance;

                    backgroundTransform.DOLocalMove(endPosition, 0.3f).SetEase(Ease.InCubic)
                        .OnComplete(CloseImmediately);
                }

                break;
            }
        }
    }

    public virtual void Close(Action closeAction = null)
    {
        if (IsClosing)
        {
            return;
        }

        IsClosing = true;

        _closeAction = closeAction;

        PlayAnimation(false);
    }

    protected virtual void OnOpened()
    {
    }

    private void CloseImmediately()
    {
        Action closeAction = _closeAction;
        _closeAction = null;

        Managers.UI.ClosePopupUI(this);

        closeAction?.Invoke();
    }

    protected virtual void OnDestroy()
    {
        transform.DOKill(true);
        DOTween.Kill(gameObject);

        DestroyOverride();
    }

    protected abstract void DestroyOverride();
}