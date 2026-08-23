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

    public RectTransform HpSliderTransform => Get<Slider>(Sliders.HpSlider).GetComponent<RectTransform>();
    
    private PlayerCtrl _player;

    protected override void OnAwake()
    {
        Bind<Slider>(typeof(Sliders));
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
        if (_player == null)
        {
            return;
        }

        _player.OnHpChanged -= UpdateHpSlider;
        _player.OnSpChanged -= UpdateSpSlider;
    }

    private void UpdateHpSlider(float currentHp, float maxHp)
    {
        if (Get<Slider>(Sliders.HpSlider) == null)
        {
            return;
        }

        Get<Slider>(Sliders.HpSlider).value = maxHp > 0f ? currentHp / maxHp : 0f;
    }

    private void UpdateSpSlider(float currentSp, float maxSp)
    {
        if (Get<Slider>(Sliders.SpSlider) == null)
        {
            return;
        }

        Get<Slider>(Sliders.SpSlider).value = maxSp > 0f ? currentSp / maxSp : 0f;
    }
}