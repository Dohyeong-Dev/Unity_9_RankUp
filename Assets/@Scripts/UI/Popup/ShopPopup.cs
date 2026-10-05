using TMPro;
using UnityEngine.UI;

public class ShopPopup : BasePopup
{
    public enum Buttons
    {
        ExitButton
    }

    public enum Texts
    {
        GoldText,
        PreparingText
    }

    public enum Toggles
    {
        EquipmentToggle,
        ConsumableToggle,
    }

    public enum Scrolls
    {
        EquipmentScroll,
        ConsumableScroll
    }

    private Toggles _currentToggle;

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Toggle>(typeof(Toggles));
        Bind<ScrollRect>(typeof(Scrolls));
    }

    protected override void OnStart()
    {
        Get<Button>(Buttons.ExitButton).onClick.AddListener(ClickExit);

        InitializeToggle();
        InitializeScroll();
        
        UpdateGoldText();
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (Managers.Input.KeyDown_Esc || Managers.Input.KeyDown_P)
        {
            ClickExit();
        }
    }

    protected override void DestroyOverride()
    {
        if (Managers.Scene != null && Managers.Scene.TryGetCurrentScene(out GameScene scene))
        {
            scene.HUD.SetSideIconActive(GameHUD.SideBar.Shop, false);
        }
    }

    #region ===== 초기화 =====

    /// <summary> 시작 시 토글 관련 이벤트를 체인 및 세팅한다. </summary>
    private void InitializeToggle()
    {
        Toggles initToggle = Toggles.ConsumableToggle;
        Get<Toggle>(initToggle).isOn = true;
        UpdateToggleScroll(initToggle);
        _currentToggle = initToggle;

        Get<Toggle>(Toggles.EquipmentToggle).onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.Button);
                UpdateToggleScroll(Toggles.EquipmentToggle);
            }
        });

        Get<Toggle>(Toggles.ConsumableToggle).onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.Button);
                UpdateToggleScroll(Toggles.ConsumableToggle);
            }
        });
    }

    /// <summary> ShopTable에 있는 아이템 리스트를 세팅한다. </summary>
    private void InitializeScroll()
    {
        // 장비
        ShopData[] sellingEquipItemList = Managers.Table.Shop.GetSellingItemList(ItemType.Equipment);

        if (sellingEquipItemList != null)
        {
            for (int i = 0; i < sellingEquipItemList.Length; i++)
            {
                Managers.UI.MakeSlot<ShopSlot>(Get<ScrollRect>(Scrolls.EquipmentScroll).content);
            }
        }

        // 소비
        ShopData[] sellingConsumableItemList = Managers.Table.Shop.GetSellingItemList(ItemType.Consumable);

        if (sellingConsumableItemList != null)
        {
            for (int i = 0; i < sellingConsumableItemList.Length; i++)
            {
                Managers.UI.MakeSlot<ShopSlot>(Get<ScrollRect>(Scrolls.ConsumableScroll).content);
            }
        }
    }

    #endregion ===== 초기화 =====

    /// <summary> 토글 변경에 따라 관련 스크롤을 갱신한다. </summary>
    private void UpdateToggleScroll(Toggles toggle)
    {
        if (toggle == _currentToggle)
        {
            return;
        }
        
        Get<ScrollRect>(Scrolls.EquipmentScroll).gameObject.SetActive(false);
        Get<ScrollRect>(Scrolls.ConsumableScroll).gameObject.SetActive(false);

        _currentToggle = toggle;

        ScrollRect scroll = Get<ScrollRect>(_currentToggle);
        scroll.gameObject.SetActive(true);

        if (scroll.content.childCount == 0)
        {
            Get<TMP_Text>(Texts.PreparingText).gameObject.SetActive(true);
        }
        Get<TMP_Text>(Texts.PreparingText).gameObject.SetActive(false);
    }

    /// <summary> 현재 보유 Gold를 UI에 표시한다. </summary>
    private void UpdateGoldText()
    {
        Get<TMP_Text>(Texts.GoldText).text = Managers.Data.Gold.ToString("N0");
    }

    /// <summary> Exit 했을 때 종료소리 재생 및 팝업을 끈다. </summary>
    private void ClickExit()
    {
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimeCancel);

        Close();
    }
}