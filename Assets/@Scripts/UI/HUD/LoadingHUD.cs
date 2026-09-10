using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 씬 로딩 진행률을 표시하고 다음 씬으로 진행할 입력을 처리하는 HUD다. </summary>
public class LoadingHUD : BaseHUD
{
    private enum Sliders
    {
        LoadingSlider,
    }

    private enum Texts
    {
        AlertTxt,
    }

    private enum Images
    {
        GlowImage,
    }

    public float LoadingProgress => Get<Slider>(Sliders.LoadingSlider).value;

    public bool ContinueRequested { get; private set; }

    private Tween _continueMessageTween;
    
    protected override void OnAwake()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));

        Get<TMP_Text>(Texts.AlertTxt).gameObject.SetActive(false);
        Get<Image>(Images.GlowImage).gameObject.SetActive(false);
    }

    protected override void OnStart()
    {
        Managers.Input.SetCursorLock(false);
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (LoadingProgress < 1f)
        {
            return;
        }

        if (Managers.Input.MouseDown_Left)
        {
            ContinueRequested = true;
        }
    }

    private void OnDisable()
    {
        _continueMessageTween?.Kill();
        _continueMessageTween = null;
    }

    /// <summary> 로딩 진행률을 설정하고 완료 시 계속 진행 안내를 표시한다. </summary>
    public void SetLoadingProgress(float progress)
    {
        Slider loadingSlider = Get<Slider>(Sliders.LoadingSlider);
        loadingSlider.value = Mathf.Clamp01(progress);

        if (loadingSlider.value >= 1f)
        {
            ShowContinueMessage();
        }
    }

    private void ShowContinueMessage()
    {
        Image glowImage = Get<Image>(Images.GlowImage);
        TMP_Text alertText = Get<TMP_Text>(Texts.AlertTxt);

        if (glowImage == null || alertText == null || alertText.gameObject.activeSelf)
        {
            return;
        }

        Get<Slider>(Sliders.LoadingSlider).gameObject.SetActive(false);

        _continueMessageTween?.Kill();

        Color glowColor = glowImage.color;
        glowColor.a = 0f;
        glowImage.color = glowColor;

        alertText.alpha = 0f;
        alertText.transform.localScale = Vector3.one * 0.95f;

        glowImage.gameObject.SetActive(true);
        alertText.gameObject.SetActive(true);

        Sequence showSequence = DOTween.Sequence();

        // Glow와 문구를 동시에 등장
        showSequence.Append(
            glowImage.DOFade(1f, 0.8f)
                .SetEase(Ease.OutSine));

        showSequence.Join(
            alertText.DOFade(1f, 0.8f)
                .SetEase(Ease.OutSine));

        showSequence.Join(
            alertText.transform.DOScale(Vector3.one, 0.8f)
                .SetEase(Ease.OutBack));

        showSequence.OnComplete(() =>
        {
            _continueMessageTween = DOTween.Sequence()
                .Append(alertText.DOFade(0.5f, 0.9f)
                    .SetEase(Ease.InOutSine))
                .Join(glowImage.DOFade(0.65f, 0.9f)
                    .SetEase(Ease.InOutSine))
                .Append(alertText.DOFade(1f, 0.9f)
                    .SetEase(Ease.InOutSine))
                .Join(glowImage.DOFade(1f, 0.9f)
                    .SetEase(Ease.InOutSine))
                .SetLoops(-1, LoopType.Restart);
        });

        _continueMessageTween = showSequence;
    }
}