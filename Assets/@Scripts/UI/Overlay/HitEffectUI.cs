using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 플레이어 피격 시 화면에 짧은 페이드 효과를 표시하는 UI다. </summary>
public class HitEffectUI : BaseOverlay
{
    public override int SortingOrder => 998;
    
    private Sequence _sequence;
    
    private enum Images
    {
        Bg
    }

    #region ===== 설정 =====

    [Header("피격 효과")]
    [SerializeField] private float _fadeInDuration = 0.05f;

    [SerializeField] private float _displayDuration = 0.08f;

    [SerializeField] private float _fadeOutDuration = 0.3f;

    #endregion ===== 설정 =====

    protected override void OnAwake()
    {
        Bind<Image>(typeof(Images));

        SetAlpha(0f);
    }

    private void OnDestroy()
    {
        _sequence?.Kill();
    }
    
    /// <summary> 피격 화면 효과를 재생한다. </summary>
    public void Play()
    {
        _sequence?.Kill();

        gameObject.SetActive(true);
        SetAlpha(0f);

        Image background = Get<Image>(Images.Bg);

        _sequence = DOTween.Sequence().Append(background.DOFade(0.7f, _fadeInDuration).SetEase(Ease.OutQuad))
            .AppendInterval(_displayDuration)
            .Append(background.DOFade(0f, _fadeOutDuration).SetEase(Ease.OutCubic))
            .OnComplete(() => gameObject.SetActive(false));
    }

    /// <summary> 피격 화면 효과를 중지한다. </summary>
    public void Stop()
    {
        _sequence?.Kill();

        gameObject.SetActive(false);
        SetAlpha(0f);
    }
    
    /// <summary> 피격 효과 이미지의 투명도를 설정한다. </summary>
    private void SetAlpha(float alpha)
    {
        Image background = Get<Image>(Images.Bg);

        Color color = background.color;
        color.a = alpha;

        background.color = color;
    }
}