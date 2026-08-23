using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HitEffectUI : BaseUI
{
    private enum Images
    {
        Bg
    }

    [Header("피격 효과")] [SerializeField] private float _fadeInDuration = 0.05f;
    [SerializeField] private float _displayDuration = 0.08f;
    [SerializeField] private float _fadeOutDuration = 0.3f;

    private Sequence _sequence;

    public override int SortingOrder => 998;

    private void Awake()
    {
        Bind<Image>(typeof(Images));

        SetAlpha(0f);
    }

    public void Play()
    {
        _sequence?.Kill();

        gameObject.SetActive(true);

        SetAlpha(0f);

        _sequence = DOTween.Sequence();

        _sequence.Append(Get<Image>(Images.Bg).DOFade(0.5f, _fadeInDuration).SetEase(Ease.OutQuad))
            .AppendInterval(_displayDuration)
            .Append(Get<Image>(Images.Bg).DOFade(0f, _fadeOutDuration).SetEase(Ease.OutCubic))
            .OnComplete(() => { gameObject.SetActive(false); });
    }

    private void SetAlpha(float alpha)
    {
        Color color = Get<Image>(Images.Bg).color;
        color.a = alpha;
        Get<Image>(Images.Bg).color = color;
    }

    private void OnDestroy()
    {
        _sequence?.Kill();
    }
}