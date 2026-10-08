using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 플레이어의 HP/SP, 보스 HP 상태등을 화면에 표시하는 게임 HUD다. </summary>
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

    private GameScene _gameScene;
    
    private PlayerCtrl _player;
    private BossCtrl _boss;
    private QuickBar _quickBar;

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

        ResolveGameScene();
        ResolveQuickBar();
        ResolvePlayer();
        
        InitializeCoverColors();

        SetBossHpSliderActive(false);
        SetPickUpActive(false);
    }
    
    protected override void OnStart()
    {
        SubscribePlayerEvents();
        SubscribeQuickBarEvents();

        UpdatePlayerStatus();
        UpdateQuickBar();
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (_player != null && _player.IsDead)
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
            Managers.UI.OpenPopup<ShopPopup>();
            SetSideIconActive(SideBar.Shop, true);
        }
        else if (Managers.Input.KeyDown_I)
        {
            Managers.UI.OpenPopup<InvenPopup>();
            SetSideIconActive(SideBar.Inventory, true);
        }
        else if (Managers.Input.KeyDown_O)
        {
            Managers.UI.OpenPopup<AlertPopup>().Set(AlertPopup.ContentsType.Save);
            SetSideIconActive(SideBar.Save, true);
        }

        OnInputQuickBar();
    }

    private void OnInputQuickBar()
    {
        if (_quickBar == null || _player == null)
        {
            return;
        }

        if (Managers.Input.KeyDown_1)
        {
            UseQuickSlotItem(0);
        }
        else if (Managers.Input.KeyDown_2)
        {
            UseQuickSlotItem(1);
        }
    }

    /// <summary> 지정된 QuickSlot에 등록된 아이템을 사용한다. </summary>
    private void UseQuickSlotItem(int quickSlotIndex)
    {
        int itemID = _quickBar.GetItemID(quickSlotIndex);

        if (itemID <= 0)
        {
            return;
        }

        if (!Managers.Data.TryUseItem(itemID))
        {
            return;
        }

        BuffType buffType = Managers.Table.Item.GetBuffType(itemID);
        int buffValue = Managers.Table.Item.GetBuffValue(itemID);

        _player.Recovery(buffType, buffValue);
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
        UnsubscribeQuickBarEvents();
        UnsubscribeBossEvents();
    }

    #region ===== 초기화 =====

    /// <summary> GameScene을 찾아 참조한다. </summary>
    private void ResolveGameScene()
    {
        _gameScene = gameObject.FindParent<GameScene>();

        if (_gameScene == null)
        {
            CPrint.Warning("[GamdHUD] GameScene을 찾을 수 없습니다.");
        }
    }
    
    /// <summary> QuickBar를 찾아 참조한다. </summary>
    private void ResolveQuickBar()
    {
        _quickBar = GetComponentInChildren<QuickBar>();

        if (_quickBar == null)
        {
            CPrint.Warning("[GamdHUD] QuickBar를 찾을 수 없습니다.");
        }
    }
    
    /// <summary> Player를 찾아 참조한다. </summary>
    private void ResolvePlayer()
    {
        _player = _gameScene.Player;

        if (_player == null)
        {
            CPrint.Warning("[GamdHUD] Player를 찾을 수 없습니다.");
        }
    }
    
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

    #region ===== 플레이어 =====

    /// <summary> 플레이어의 현재 HP와 SP를 HUD에 반영한다. </summary>
    private void UpdatePlayerStatus()
    {
        if (_player == null)
        {
            return;
        }
        
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

    #region ===== QuickBar =====

    /// <summary> 저장된 QuickSlot 데이터를 GameHUD의 QuickBar에 반영한다. </summary>
    private void UpdateQuickBar()
    {
        _quickBar?.UpdateUI();
    }

    #endregion ===== QuickBar =====

    #region ===== 이펙트 =====

    /// <summary> HP가 낮을 때 HP 게이지의 경고 효과를 재생한다. </summary>
    private void PlayHpEmptyWarning()
    {
        if (_hpEmptyCoverTween != null && _hpEmptyCoverTween.IsActive())
        {
            return;
        }

        Image hpCover = Get<Image>(Images.HpSliderCover);
        _hpEmptyCoverTween = hpCover.DOColor(Color.red, 0.25f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
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
        _spEmptyCoverTween = spCover.DOColor(Color.red, 0.25f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
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

    /// <summary> 컷씬의 보스 이벤트를 등록한다. </summary>
    public void SubscribeBossEvents(BossCtrl boss)
    {
        if (_boss == null)
        {
            _boss = boss;
        }

        _boss.OnHpChanged += UpdateBossHpSlider;
    }

    /// <summary> 컷씬의 보스 이벤트를 해제한다. </summary>
    private void UnsubscribeBossEvents()
    {
        if (_boss == null)
        {
            return;
        }

        _boss.OnHpChanged -= UpdateBossHpSlider;
    }
    
    /// <summary> 플레이어 이벤트를 등록한다. </summary>
    private void SubscribePlayerEvents()
    {
        if (_player == null)
        {
            return;
        }
        
        _player.OnHpChanged += UpdateHpSlider;
        _player.OnSpChanged += UpdateSpSlider;
    }

    /// <summary> 플레이어 이벤트를 해제한다. </summary>
    private void UnsubscribePlayerEvents()
    {
        if (_player == null)
        {
            return;
        }
        
        _player.OnHpChanged -= UpdateHpSlider;
        _player.OnSpChanged -= UpdateSpSlider;
    }

    /// <summary> 퀵바의 이벤트를 등록한다. </summary>
    private void SubscribeQuickBarEvents()
    {
        if (Managers.Data == null)
        {
            return;
        }
        
        Managers.Data.OnQuickSlotChanged += UpdateQuickBar;
    }
    
    /// <summary> 퀵바의 이벤트를 해제한다. </summary>
    private void UnsubscribeQuickBarEvents()
    {
        if (Managers.Data == null)
        {
            return;
        }
        
        Managers.Data.OnQuickSlotChanged -= UpdateQuickBar;
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