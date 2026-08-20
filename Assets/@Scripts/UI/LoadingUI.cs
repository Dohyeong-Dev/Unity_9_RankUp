using DG.Tweening;
using UnityEngine.UI;

public class LoadingUI : BaseUI
{
    private enum Images
    {
        BG,
    }

    private void Awake()
    {
        Managers.UI.SetupCanvas(gameObject, this);
        Bind<Image>(typeof(Images));
    }

    public void FadeIn(float fadeTime)
    {
        Get<Image>(Images.BG).DOKill();
        Get<Image>(Images.BG).DOFade(1f, fadeTime).From(0f);
    }

    public void FadeOut(float fadeTime)
    {
        Get<Image>(Images.BG).DOKill();
        Get<Image>(Images.BG).DOFade(0f, fadeTime).From(1f).OnComplete(() =>
        {
            Managers.UI.CloseLoadingUI();
        });
    }
}