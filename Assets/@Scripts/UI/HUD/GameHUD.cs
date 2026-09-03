using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : BaseHUD
{
    private enum Sliders
    {
        HpSlider,
        SpSlider,
    }

    private enum Texts
    {
        HpText,
        SpText,
    }

    private enum Images
    {
        SpSliderCover
    }
    
    private Tween _spEmptyCoverTween;
    private Color _spCoverDefaultColor;

    private PlayerCtrl _player;

    protected override void OnAwake()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        
        if (_spCoverDefaultColor == default(Color))
        {
            _spCoverDefaultColor = Get<Image>(Images.SpSliderCover).color;
        }
    }

    protected override void OnStart()
    {
        Managers.Input.SetCursorLock(true);

        _player = FindFirstObjectByType<PlayerCtrl>();

        if (_player == null)
        {
            CPrint.Warning("Player를 찾을 수 없습니다.");
            return;
        }

        _player.OnHpChanged += UpdateHpSlider;
        _player.OnSpChanged += UpdateSpSlider;

        // HUD가 생성된 시점의 초기값 반영
        UpdateHpSlider(_player.HP, _player.MaxHP);
        UpdateSpSlider(_player.SP, _player.MaxSP);
    }

    protected override void OnUpdate()
    {
    }

    private void OnDestroy()
    {
        _spEmptyCoverTween?.Kill();
        
        if (_player == null)
        {
            return;
        }

        _player.OnHpChanged -= UpdateHpSlider;
        _player.OnSpChanged -= UpdateSpSlider;
    }

    private void UpdateHpSlider(float currentHp, float maxHp)
    {
        Slider hpSlider = Get<Slider>(Sliders.HpSlider);

        if (hpSlider == null)
        {
            return;
        }

        float sliderValue = maxHp > 0f ? Mathf.Clamp01(currentHp / maxHp) : 0f;

        hpSlider.value = sliderValue;
        Get<TMP_Text>(Texts.HpText).text = (sliderValue * 100f).ToString("F0") + "%";
    }

    private void UpdateSpSlider(float currentSp, float maxSp)
    {
        Slider spSlider = Get<Slider>(Sliders.SpSlider);

        if (spSlider == null)
        {
            return;
        }

        float sliderValue = maxSp > 0f ? Mathf.Clamp01(currentSp / maxSp) : 0f;

        spSlider.value = sliderValue;
        Get<TMP_Text>(Texts.SpText).text = (sliderValue * 100f).ToString("F0") + "%";

        if (currentSp >= _player.SpRecoveryThreshold)
        {
            StopSpEmptyWarning();
        }
        else
        {
            PlaySpEmptyWarning();
        }
    }
    
    private void PlaySpEmptyWarning()
    {
        if (_spEmptyCoverTween != null && _spEmptyCoverTween.IsActive())
        {
            return;
        }

        _spEmptyCoverTween = Get<Image>(Images.SpSliderCover).DOColor(Color.red, 0.25f)
            .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }
    
    private void StopSpEmptyWarning()
    {
        _spEmptyCoverTween?.Kill();
        _spEmptyCoverTween = null;

        Get<Image>(Images.SpSliderCover).color = _spCoverDefaultColor;
    }
}