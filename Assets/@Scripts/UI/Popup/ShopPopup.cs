using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    
    private ItemDescription _itemDescription;

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Toggle>(typeof(Toggles));
        Bind<ScrollRect>(typeof(Scrolls));
        
        _itemDescription = GetComponentInChildren<ItemDescription>();
        if (_itemDescription == null)
        {
            CPrint.Warning("[ShopPopup] no item description found.");
        }
    }

    protected override void OnStart()
    {
        Get<Button>(Buttons.ExitButton).onClick.AddListener(ClickExit);

        InitializeScroll();
        InitializeToggle();
        
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
        UpdateToggleScroll(Toggles.ConsumableToggle);
        Get<Toggle>(_currentToggle).isOn = true;
        
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
        var sellingEquipItemIdList = Managers.Table.Shop.GetSellingItemIdList(ItemType.Equipment);

        if (sellingEquipItemIdList != null)
        {
            foreach (var itemID in sellingEquipItemIdList)
            {
                ShopSlot shopSlot = Managers.UI.MakeSlot<ShopSlot>(Get<ScrollRect>(Scrolls.EquipmentScroll).content);
                shopSlot.gameObject.BindEvent(GlobalEnum.EventType.PointerEnter, () => ShowItemDescription(shopSlot));
                shopSlot.gameObject.BindEvent(GlobalEnum.EventType.PointerExit, HideItemDescription);
                shopSlot.SetData(itemID);
                shopSlot.UpdateUI();
            }
        }

        // 소비
        var sellingConsumableItemIdList = Managers.Table.Shop.GetSellingItemIdList(ItemType.Consumable);

        if (sellingConsumableItemIdList != null)
        {
            foreach (var itemID in sellingConsumableItemIdList)
            {
                ShopSlot shopSlot = Managers.UI.MakeSlot<ShopSlot>(Get<ScrollRect>(Scrolls.ConsumableScroll).content);
                shopSlot.gameObject.BindEvent(GlobalEnum.EventType.PointerEnter, () => ShowItemDescription(shopSlot));
                shopSlot.gameObject.BindEvent(GlobalEnum.EventType.PointerExit, HideItemDescription);
                shopSlot.SetData(itemID);
                shopSlot.UpdateUI();
            }
        }
    }

    #endregion ===== 초기화 =====

    #region ===== 아이템 설명창 =====
    
    private void ShowItemDescription(ShopSlot slot)
    {
        _itemDescription?.Show(slot);
    }

    private void HideItemDescription()
    {
        _itemDescription?.Hide();
    }
    
    #endregion ===== 아이템 설명창 =====
    
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
        
        Get<TMP_Text>(Texts.PreparingText).gameObject.SetActive(scroll.content.childCount == 0);
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