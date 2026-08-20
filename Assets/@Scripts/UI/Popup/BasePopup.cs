using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class BasePopup : BaseUI
{
    protected bool IsClosing;

    public enum AnimationType
    {
        None,
        ContentsUp, // 컨텐츠가 살짝 올라옴
        BgUp,       // 배경이 살짝 올라옴
    }
    public AnimationType CurrentAnimation;

    private void Awake()
    {
        OnAwake();

        Managers.UI.SetupCanvas(gameObject, this);

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
                            contentText
                                .DOFade(1f, 0.5f)
                                .From(0f)
                                .SetEase(Ease.InOutCirc);
                        }).OnComplete(OnOpened);
                }
                else
                {
                    CloseImmediately();
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
                        .SetRelative(true).From(backgroundTransform.localPosition + Vector3.down * offsetY).OnStart(() =>
                        {
                            backgroundCanvasGroup.DOFade(0.98f, 0.13f).From(0f);
                        }).OnComplete(OnOpened);
                }
                else
                {
                    CloseImmediately();
                }

                break;
            }
        }
    }

    public virtual void Close()
    {
        if (IsClosing)
        {
            return;
        }

        IsClosing = true;

        PlayAnimation(false);
    }

    protected virtual void OnOpened()
    {
    }

    private void CloseImmediately()
    {
        Managers.UI.ClosePopupUI(this);
    }

    protected virtual void OnDestroy()
    {
        transform.DOKill(true);
        DOTween.Kill(gameObject);
        
        DestroyOverride();
    }

    protected abstract void DestroyOverride();
}