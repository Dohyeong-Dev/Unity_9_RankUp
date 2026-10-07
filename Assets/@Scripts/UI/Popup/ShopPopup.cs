using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/// <summary> 상점의 판매중인 아이템 목록을 관리하는 Popup UI다. </summary>
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

    #region ===== 상태 =====

    private Toggles _currentToggle;

    #endregion ===== 상태 =====

    #region ===== 참조 =====

    private ItemDescription _itemDescription;

    #endregion ===== 참조 =====

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Toggle>(typeof(Toggles));
        Bind<ScrollRect>(typeof(Scrolls));

        ResolveItemDescription();
    }

    protected override void OnStart()
    {
        InitializeExitButton();
        CreateShopSlots();
        InitializeCategoryToggles();

        UpdateShopUI();
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (Managers.Input.KeyDown_Esc || Managers.Input.KeyDown_P)
        {
            OnClickExit();
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
    
    /// <summary> 아이템 설명창을 찾아 참조한다. </summary>
    private void ResolveItemDescription()
    {
        _itemDescription = GetComponentInChildren<ItemDescription>();

        if (_itemDescription == null)
        {
            CPrint.Warning("[ShopPopup] ItemDescription을 찾을 수 없습니다.");
        }
    }
    
    /// <summary> Exit 버튼 이벤트를 초기화한다. </summary>
    private void InitializeExitButton()
    {
        Get<Button>(Buttons.ExitButton).onClick.AddListener(OnClickExit);
    }

    /// <summary> 현재 판매 중인 모든 아이템의 상점 슬롯을 생성한다. </summary>
    private void CreateShopSlots()
    {
        CreateShopSlotsByType(ItemType.Equipment, Get<ScrollRect>(Scrolls.EquipmentScroll));
        CreateShopSlotsByType(ItemType.Consumable, Get<ScrollRect>(Scrolls.ConsumableScroll));
    }

    /// <summary> 지정된 아이템 타입의 판매 슬롯을 생성한다. </summary>
    private void CreateShopSlotsByType(ItemType itemType, ScrollRect scroll)
    {
        IReadOnlyList<int> itemIDList = Managers.Table.Shop.GetSellingItemIdList(itemType);

        if (itemIDList == null || itemIDList.Count == 0)
        {
            return;
        }

        foreach (int itemID in itemIDList)
        {
            ShopSlot slot = Managers.UI.MakeSlot<ShopSlot>(scroll.content);

            if (slot == null)
            {
                CPrint.Error($"[ShopPopup] ShopSlot 생성 실패. ItemID: {itemID}");
                continue;
            }

            slot.SetData(itemID, this);
            slot.UpdateUI();

            BindShopSlotEvents(slot);
        }
    }

    /// <summary> 상점 카테고리 토글 이벤트와 초기 상태를 설정한다. </summary>
    private void InitializeCategoryToggles()
    {
        Get<Toggle>(Toggles.EquipmentToggle).onValueChanged.AddListener(isOn => 
            OnCategoryToggleChanged(Toggles.EquipmentToggle, isOn));

        Get<Toggle>(Toggles.ConsumableToggle).onValueChanged.AddListener(isOn => 
            OnCategoryToggleChanged(Toggles.ConsumableToggle, isOn));

        SetCategory(Toggles.EquipmentToggle);
    }

    #endregion ===== 초기화 =====

    #region ===== 활성화/비활성화 =====

    /// <summary> 지정된 상점 카테고리를 활성화한다. </summary>
    private void SetCategory(Toggles toggle)
    {
        HideAllShopScrolls();

        _currentToggle = toggle;

        ScrollRect scroll = Get<ScrollRect>(_currentToggle);

        scroll.gameObject.SetActive(true);

        UpdatePreparingText(scroll);
    }

    /// <summary> 모든 상점 스크롤을 비활성화한다. </summary>
    private void HideAllShopScrolls()
    {
        Get<ScrollRect>(Scrolls.EquipmentScroll).gameObject.SetActive(false);
        Get<ScrollRect>(Scrolls.ConsumableScroll).gameObject.SetActive(false);
    }

    /// <summary> 지정된 슬롯의 아이템 설명을 표시한다. </summary>
    private void ShowItemDescription(ShopSlot slot)
    {
        if (_itemDescription == null)
        {
            return;
        }

        _itemDescription.Show(slot);
    }

    /// <summary> 아이템 설명을 숨긴다. </summary>
    private void HideItemDescription()
    {
        if (_itemDescription == null)
        {
            return;
        }

        _itemDescription.Hide();
    }
    
    #endregion ===== 활성화/비활성화 =====

    #region ===== 갱신 =====

    /// <summary> 상점 UI의 상태를 갱신한다. </summary>
    public void UpdateShopUI()
    {
        UpdateGoldText();
    }
    
    /// <summary> 현재 카테고리의 판매 아이템 준비 문구를 갱신한다. </summary>
    private void UpdatePreparingText(ScrollRect scroll)
    {
        bool hasShopItem = scroll.content.childCount > 0;

        Get<TMP_Text>(Texts.PreparingText).gameObject.SetActive(!hasShopItem);
    }
    
    /// <summary> 현재 보유 Gold를 UI에 표시한다. </summary>
    private void UpdateGoldText()
    {
        Get<TMP_Text>(Texts.GoldText).text = Managers.Data.Gold.ToString("N0");
    }

    #endregion ===== 갱신 =====

    #region ===== 이벤트 =====

    /// <summary> 상점 카테고리 토글 변경을 처리한다. </summary>
    private void OnCategoryToggleChanged(Toggles toggle, bool isOn)
    {
        if (!isOn)
        {
            return;
        }

        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.Button);

        SetCategory(toggle);
    }
    
    /// <summary> Exit 버튼을 눌렀을 때 상점 Popup을 닫는다. </summary>
    private void OnClickExit()
    {
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimeCancel);

        Close();
    }
    
    /// <summary> ShopSlot의 마우스 오버 이벤트를 연결한다. </summary>
    private void BindShopSlotEvents(ShopSlot slot)
    {
        slot.gameObject.BindEvent(UIEventType.PointerEnter, () => ShowItemDescription(slot));
        slot.gameObject.BindEvent(UIEventType.PointerExit, HideItemDescription);
    }

    #endregion ===== 이벤트 =====
}