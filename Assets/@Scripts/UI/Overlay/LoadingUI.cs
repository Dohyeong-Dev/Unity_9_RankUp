using System;
using DG.Tweening;
using UnityEngine.UI;

/// <summary> 씬 전환 중 화면을 가리고 페이드 효과를 처리하는 로딩 UI다. </summary>
public class LoadingUI : BaseOverlay
{
    public override int SortingOrder => 999;
    
    private enum Images
    {
        Bg,
    }

    protected override void OnAwake()
    {
        Bind<Image>(typeof(Images));
    }

    /// <summary> 로딩 UI를 페이드 인하고 완료 후 지정된 동작을 실행한다. </summary>
    public void FadeIn(float fadeTime, Action completionAction = null)
    {
        Image background = Get<Image>(Images.Bg);

        background.DOKill();

        background.DOFade(1f, fadeTime).From(0f).SetEase(Ease.InQuad)
            .OnComplete(() => completionAction?.Invoke());
    }

    /// <summary> 로딩 UI를 페이드 아웃하고 완료 후 지정된 동작을 실행한다. </summary>
    public void FadeOut(float fadeTime, Action completionAction = null)
    {
        Image background = Get<Image>(Images.Bg);

        background.DOKill();

        Sequence sequence = DOTween.Sequence();

        sequence.AppendInterval(0.1f);
        sequence.Append(background.DOFade(0f, fadeTime).From(1f).SetEase(Ease.OutQuad));
        sequence.OnComplete(() =>
        {
            completionAction?.Invoke();
            Managers.UI.CloseLoadingUI();
        });
    }
}