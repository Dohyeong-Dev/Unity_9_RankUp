using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 플레이어의 HP/SP, 보스 HP 상태를 화면에 표시하는 게임 HUD다. </summary>
public class GameHUD : BaseHUD
{
    private enum Sliders
    {
        HpSlider,
        SpSlider,
        BossHpSlider
    }

    private enum Texts
    {
        HpText,
        SpText,
        PickUpText
    }

    private enum Images
    {
        HpSliderCover,
        SpSliderCover,
        
        SettingIconFrame,
        SettingIcon,
        ShopIconFrame,
        ShopIcon,
        InventoryIconFrame,
        InventoryIcon,
        SaveIconFrame,
        SaveIcon,
        
        PickUp
    }

    public enum SideBar
    {
        Setting,
        Shop,
        Inventory,
        Save
    }

    private readonly Dictionary<SideBar, bool> _sideIconStatusMap = new();

    #region ===== 참조 =====

    private PlayerCtrl _player;
    private EnemyCtrl _boss;

    #endregion ===== 참조 =====

    #region ===== 이펙트 =====

    private Tween _hpEmptyCoverTween;
    private Tween _spEmptyCoverTween;
    private Tween _bossHpShowTween;

    private Color _hpCoverDefaultColor;
    private Color _spCoverDefaultColor;

    #endregion ===== 이펙트 =====

    protected override void OnAwake()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));

        InitializeCoverColors();

        SetBossHpSliderActive(false);
        SetPickUpActive(false);
    }

    protected override void OnStart()
    {
        if (!TryResolvePlayer())
        {
            return;
        }

        SubscribePlayerEvents();
        UpdatePlayerStatus();
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (_player.IsDead)
        {
            return;
        }
        
        if (Managers.Input.KeyDown_Esc)
        {
            Managers.UI.OpenPopup<ExitPopup>();
            SetSideIconActive(SideBar.Setting, true);
        }
        else if (Managers.Input.KeyDown_P)
        {
            Managers.UI.OpenToastMessage("상점");
        }
        else if (Managers.Input.KeyDown_I)
        {
            Managers.UI.OpenPopup<InventoryPopup>();
            SetSideIconActive(SideBar.Inventory, true);
        }
        else if (Managers.Input.KeyDown_O)
        {
            Managers.UI.OpenPopup<AlertPopup>().Set(AlertPopup.ContentsType.Save);
            SetSideIconActive(SideBar.Save, true);
        }
    }

    private void OnDestroy()
    {
        _hpEmptyCoverTween?.Kill();
        _spEmptyCoverTween?.Kill();

        if (_player == null)
        {
            return;
        }

        UnsubscribePlayerEvents();
        UnsubscribeBossEvents();
    }

    #region ===== 초기화 =====

    /// <summary> HP와 SP 게이지 커버의 기본 색상을 저장한다. </summary>
    private void InitializeCoverColors()
    {
        _hpCoverDefaultColor = Get<Image>(Images.HpSliderCover).color;
        _spCoverDefaultColor = Get<Image>(Images.SpSliderCover).color;
    }

    #endregion ===== 초기화 =====

    #region ===== 활성/비활성화 =====

    private void SetBossHpSliderActive(bool active)
    {
        Get<Slider>(Sliders.BossHpSlider).gameObject.SetActive(active);
    }

    public void SetPickUpActive(bool active, string message = null)
    {
        Get<Image>(Images.PickUp).gameObject.SetActive(active);
        Get<TMP_Text>(Texts.PickUpText).text = message;
    }

    #endregion ===== 활성/비활성화 =====

    #region ===== 참조 확인 =====

    /// <summary> 플레이어 참조를 반환하고 없으면 현재 씬에서 찾는다. </summary>
    private bool TryResolvePlayer()
    {
        if (_player != null)
        {
            return true;
        }

        _player = FindFirstObjectByType<PlayerCtrl>();

        if (_player != null)
        {
            return true;
        }

        CPrint.Warning("Player를 찾을 수 없습니다.");
        return false;
    }

    #endregion ===== 참조 확인 =====

    #region ===== 플레이어 =====

    /// <summary> 플레이어의 현재 HP와 SP를 HUD에 반영한다. </summary>
    private void UpdatePlayerStatus()
    {
        UpdateHpSlider(_player.HP, _player.MaxHP);
        UpdateSpSlider(_player.SP, _player.MaxSP);
    }

    /// <summary> HP UI를 현재 HP 비율에 맞게 갱신한다. </summary>
    private void UpdateHpSlider(float currentHp, float maxHp)
    {
        Slider hpSlider = Get<Slider>(Sliders.HpSlider);

        if (hpSlider == null)
        {
            return;
        }

        float sliderValue = maxHp > 0f ? Mathf.Clamp01(currentHp / maxHp) : 0f;

        hpSlider.value = sliderValue;
        Get<TMP_Text>(Texts.HpText).text = $"{sliderValue * 100f:F0}%";

        if (sliderValue <= 0.1f)
        {
            PlayHpEmptyWarning();
            return;
        }

        StopHpEmptyWarning();
    }

    /// <summary> SP UI를 현재 SP 비율에 맞게 갱신한다. </summary>
    private void UpdateSpSlider(float currentSp, float maxSp)
    {
        Slider spSlider = Get<Slider>(Sliders.SpSlider);

        if (spSlider == null)
        {
            return;
        }

        float sliderValue = maxSp > 0f ? Mathf.Clamp01(currentSp / maxSp) : 0f;

        spSlider.value = sliderValue;
        Get<TMP_Text>(Texts.SpText).text = $"{sliderValue * 100f:F0}%";

        if (currentSp >= _player.SpActionResumeThreshold)
        {
            StopSpEmptyWarning();
            return;
        }

        PlaySpEmptyWarning();
    }

    #endregion ===== 플레이어 =====

    #region ===== 보스 =====

    private void UpdateBossHpSlider(float currentHp, float maxHp)
    {
        Slider bossHpSlider = Get<Slider>(Sliders.BossHpSlider);

        if (_bossHpShowTween == null)
        {
            SetBossHpSliderActive(true);

            bossHpSlider.transform.localScale = Vector3.zero;

            _bossHpShowTween = bossHpSlider.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }

        float sliderValue = maxHp > 0f ? Mathf.Clamp01(currentHp / maxHp) : 0f;
        bossHpSlider.value = sliderValue;
    }

    #endregion ===== 보스 =====

    #region ===== 사이드바 =====

    /// <summary> 해당 사이드 아이콘의 활성화 상태를 갱신한다. </summary>
    public void SetSideIconActive(SideBar sideBar, bool isActive)
    {
        _sideIconStatusMap[sideBar] = isActive;

        switch (sideBar)
        {
            case SideBar.Setting:
                UpdateSideIconColor(Images.SettingIconFrame, sideBar);
                UpdateSideIconColor(Images.SettingIcon, sideBar);
                break;

            case SideBar.Shop:
                UpdateSideIconColor(Images.ShopIconFrame, sideBar);
                UpdateSideIconColor(Images.ShopIcon, sideBar);
                break;

            case SideBar.Inventory:
                UpdateSideIconColor(Images.InventoryIconFrame, sideBar);
                UpdateSideIconColor(Images.InventoryIcon, sideBar);
                break;

            case SideBar.Save:
                UpdateSideIconColor(Images.SaveIconFrame, sideBar);
                UpdateSideIconColor(Images.SaveIcon, sideBar);
                break;
        }
    }

    /// <summary> 사이드 아이콘 컬러를 갱신한다. </summary>
    private void UpdateSideIconColor(Images image, SideBar sideBar)
    {
        Get<Image>(image).color = _sideIconStatusMap[sideBar] ? ColorKey.Charcoal : Color.white;
    }

    #endregion ===== 사이드바 =====

    #region ===== 이펙트 =====

    /// <summary> HP가 낮을 때 HP 게이지의 경고 효과를 재생한다. </summary>
    private void PlayHpEmptyWarning()
    {
        if (_hpEmptyCoverTween != null && _hpEmptyCoverTween.IsActive())
        {
            return;
        }

        Image hpCover = Get<Image>(Images.HpSliderCover);

        _hpEmptyCoverTween = hpCover.DOColor(Color.red, 0.25f).SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    /// <summary> HP 게이지의 경고 효과를 중지하고 기본 색상으로 복원한다. </summary>
    private void StopHpEmptyWarning()
    {
        _hpEmptyCoverTween?.Kill();
        _hpEmptyCoverTween = null;

        Get<Image>(Images.HpSliderCover).color = _hpCoverDefaultColor;
    }

    /// <summary> SP가 부족할 때 SP 게이지의 경고 효과를 재생한다. </summary>
    private void PlaySpEmptyWarning()
    {
        if (_spEmptyCoverTween != null && _spEmptyCoverTween.IsActive())
        {
            return;
        }

        Image spCover = Get<Image>(Images.SpSliderCover);

        _spEmptyCoverTween = spCover.DOColor(Color.red, 0.25f).SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    /// <summary> SP 게이지의 경고 효과를 중지하고 기본 색상으로 복원한다. </summary>
    private void StopSpEmptyWarning()
    {
        _spEmptyCoverTween?.Kill();
        _spEmptyCoverTween = null;

        Get<Image>(Images.SpSliderCover).color = _spCoverDefaultColor;
    }

    #endregion ===== 이펙트 =====

    #region ===== 이벤트 =====

    /// <summary> 플레이어 이벤트를 등록하고 현재 스탯을 HUD에 반영한다. </summary>
    private void SubscribePlayerEvents()
    {
        _player.OnHpChanged += UpdateHpSlider;
        _player.OnSpChanged += UpdateSpSlider;
    }

    /// <summary> 플레이어 이벤트를 해제한다. </summary>
    private void UnsubscribePlayerEvents()
    {
        _player.OnHpChanged -= UpdateHpSlider;
        _player.OnSpChanged -= UpdateSpSlider;
    }

    public void SubscribeBossEvents(EnemyCtrl boss)
    {
        if (_boss == null)
        {
            _boss = boss;
        }

        _boss.OnHpChanged += UpdateBossHpSlider;
    }

    private void UnsubscribeBossEvents()
    {
        if (_boss == null)
        {
            return;
        }

        _boss.OnHpChanged -= UpdateBossHpSlider;
    }

    #endregion ===== 이벤트 =====

    #region ===== 상호작용 =====

    public void SetInteractionTextActive(bool isActive, InteractType type = InteractType.PickUp)
    {
        SetPickUpActive(isActive, GetInteractionText(type));
    }

    private string GetInteractionText(InteractType type)
    {
        string interactionText = "[F] ";

        interactionText += type switch
        {
            InteractType.PickUp => "줍기",
            _ => string.Empty
        };

        return interactionText;
    }

    #endregion ===== 상호작용 =====
}