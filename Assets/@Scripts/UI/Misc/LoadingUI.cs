using System;
using DG.Tweening;
using UnityEngine.UI;

public class LoadingUI : BaseUI
{
    public override int SortingOrder => 999;
    
    private enum Images
    {
        BG,
    }

    private void Awake()
    {
        Managers.UI.SetupCanvas(this);
        Bind<Image>(typeof(Images));
    }

    public void FadeIn(float fadeTime, Action completionAction = null)
    {
        Get<Image>(Images.BG).DOKill();
        Get<Image>(Images.BG).DOFade(1f, fadeTime).From(0f).SetEase(Ease.InQuad)
            .OnComplete(() =>
        {
            completionAction?.Invoke();
        });
    }

    public void FadeOut(float fadeTime, Action completionAction = null)
    {
        Image background = Get<Image>(Images.BG);

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