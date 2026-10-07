using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 인벤토리의 아이템 목록과 카테고리 전환을 관리하는 Popup UI다. </summary>
public class InvenPopup : BasePopup
{
    public enum Buttons
    {
        ExitButton
    }

    public enum Texts
    {
        GoldText
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

    #region ===== 설정 =====

    [Header("설정")]
    [SerializeField, Min(1)]
    private int _equipmentSlotCount = 20;

    [SerializeField, Min(1)]
    private int _consumableSlotCount = 20;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private InvenSlot[] _equipmentSlots;
    private InvenSlot[] _consumableSlots;

    #endregion ===== 설정 =====

    #region ===== 참조 =====

    private ItemDescription _itemDescription;
    private GameObject _quickBar;

    #endregion ===== 참조 =====

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Toggle>(typeof(Toggles));
        Bind<ScrollRect>(typeof(Scrolls));

        ResolveItemDescription();
        ResolveQuickBar();
    }

    protected override void OnStart()
    {
        InitializeExitButton();
        CreateInvenSlots();
        InitializeCategoryToggles();

        UpdateInvenUI();
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (Managers.Input.KeyDown_Esc || Managers.Input.KeyDown_I)
        {
            OnClickExit();
        }
    }

    protected override void OnOpened()
    {
        base.OnOpened();

        SetQuickBarActive(true);
    }

    protected override void DestroyOverride()
    {
        if (Managers.Scene != null && Managers.Scene.TryGetCurrentScene(out GameScene scene))
        {
            scene.HUD.SetSideIconActive(GameHUD.SideBar.Inventory, false);
        }
    }
    
    #region ===== 초기화 =====
    
    /// <summary> 아이템 설명창을 찾아 참조한다. </summary>
    private void ResolveItemDescription()
    {
        _itemDescription = GetComponentInChildren<ItemDescription>();

        if (_itemDescription == null)
        {
            CPrint.Warning("[InventoryPopup] ItemDescription을 찾을 수 없습니다.");
        }
    }
    
    /// <summary> QuickBar를 찾아 참조한다. </summary>
    private void ResolveQuickBar()
    {
        HorizontalLayoutGroup quickBar = gameObject.FindChild<HorizontalLayoutGroup>("QuickBar");

        if (quickBar == null)
        {
            CPrint.Warning("[InventoryPopup] QuickBar를 찾을 수 없습니다.");
            return;
        }

        _quickBar = quickBar.gameObject;
        SetQuickBarActive(false);
    }
    
    /// <summary> Exit 버튼 이벤트를 초기화한다. </summary>
    private void InitializeExitButton()
    {
        Get<Button>(Buttons.ExitButton).onClick.AddListener(OnClickExit);
    }

    /// <summary> 인벤토리 슬롯들을 생성한다. </summary>
    private void CreateInvenSlots()
    {
        CreateInvenSlotsByType(ItemType.Equipment, Get<ScrollRect>(Scrolls.EquipmentScroll));
        CreateInvenSlotsByType(ItemType.Consumable, Get<ScrollRect>(Scrolls.ConsumableScroll));
    }

    /// <summary> 아이템 타입에 따라 지정된 개수만큼 인벤토리 슬롯 배열을 생성한다. </summary>
    private void CreateInvenSlotsByType(ItemType itemType, ScrollRect scroll)
    {
        InvenSlot[] slotList = itemType switch
        {
            ItemType.Equipment => _equipmentSlots = new InvenSlot[_equipmentSlotCount],
            ItemType.Consumable => _consumableSlots = new InvenSlot[_consumableSlotCount],
            _ => null
        };

        if (slotList == null)
        {
            return;
        }

        for (int i = 0; i < slotList.Length; i++)
        {
            InvenSlot slot = Managers.UI.MakeSlot<InvenSlot>(scroll.content);

            if (slot == null)
            {
                CPrint.Error($"[InventoryPopup] InvenSlot 생성 실패. Index: {i}");
                continue;
            }

            slotList[i] = slot;
        }
    }

    /// <summary> 인벤토리 카테고리 토글 이벤트와 초기 상태를 설정한다. </summary>
    private void InitializeCategoryToggles()
    {
        Get<Toggle>(Toggles.EquipmentToggle).onValueChanged.AddListener(isOn =>
            OnCategoryToggleChanged(Toggles.EquipmentToggle, isOn));

        Get<Toggle>(Toggles.ConsumableToggle).onValueChanged.AddListener(isOn =>
            OnCategoryToggleChanged(Toggles.ConsumableToggle, isOn));

        SetCategoryActive(Toggles.EquipmentToggle);
    }

    #endregion ===== 초기화 =====

    #region ===== 갱신 =====

    /// <summary> 인벤토리 슬롯과 Gold UI를 갱신한다. </summary>
    public void UpdateInvenUI()
    {
        UpdateInvenSlots(ItemType.Equipment);
        UpdateInvenSlots(ItemType.Consumable);

        UpdateGoldText();
    }

    /// <summary> 지정된 타입의 아이템을 인벤토리 슬롯에 배치한다. </summary>
    private void UpdateInvenSlots(ItemType itemType)
    {
        InvenSlot[] slotList = itemType switch
        {
            ItemType.Equipment => _equipmentSlots,
            ItemType.Consumable => _consumableSlots,
            _ => null
        };

        if (slotList == null)
        {
            return;
        }
        
        List<InventoryItemData> inventoryItems = GetInventoryItems(itemType);

        for (int i = 0; i < slotList.Length; i++)
        {
            InvenSlot slot = slotList[i];

            if (i < inventoryItems.Count)
            {
                InventoryItemData itemData = inventoryItems[i];

                slot.SetData(itemData.ItemID, itemData.Count);
                BindInvenSlotEvents(slot);
            }
            else
            {
                slot.ClearData();
                ClearInvenSlotEvents(slot);
            }

            slot.UpdateUI();
        }
    }

    /// <summary> 현재 보유 Gold를 UI에 표시한다. </summary>
    private void UpdateGoldText()
    {
        Get<TMP_Text>(Texts.GoldText).text = Managers.Data.Gold.ToString("N0");
    }

    #endregion ===== 갱신 =====

    #region ===== 활성화/비활성화 =====

    /// <summary> 지정된 인벤토리 카테고리를 활성화한다. </summary>
    private void SetCategoryActive(Toggles toggle)
    {
        HideAllInventoryScrolls();

        ScrollRect scroll = Get<ScrollRect>(toggle);
        scroll.gameObject.SetActive(true);

        Toggle categoryToggle = Get<Toggle>(toggle);
        if (!categoryToggle.isOn)
        {
            categoryToggle.isOn = true;
        }
    }

    /// <summary> 모든 인벤토리 스크롤을 비활성화한다. </summary>
    private void HideAllInventoryScrolls()
    {
        Get<ScrollRect>(Scrolls.EquipmentScroll).gameObject.SetActive(false);
        Get<ScrollRect>(Scrolls.ConsumableScroll).gameObject.SetActive(false);
    }
    
    /// <summary> QuickBar의 활성화 상태를 설정한다. </summary>
    private void SetQuickBarActive(bool isActive)
    {
        if (_quickBar == null)
        {
            return;
        }

        _quickBar.SetActive(isActive);
    }

    /// <summary> 지정된 슬롯의 아이템 설명을 표시한다. </summary>
    private void ShowItemDescription(InvenSlot slot)
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

    #region ===== Get =====

    /// <summary> 지정된 타입에 해당하는 보유 아이템 목록을 반환한다. </summary>
    private List<InventoryItemData> GetInventoryItems(ItemType itemType)
    {
        List<InventoryItemData> inventoryItems = new();

        IReadOnlyList<InventoryItemData> inventoryItemList = Managers.Data.GetInventoryItemList();

        foreach (InventoryItemData inventoryItem in inventoryItemList)
        {
            if (Managers.Table.Item.GetItemType(inventoryItem.ItemID) != itemType)
            {
                continue;
            }

            inventoryItems.Add(inventoryItem);
        }

        return inventoryItems;
    }

    #endregion ===== Get =====

    #region ===== 이벤트 =====

    /// <summary> 카테고리 토글이 활성화 됐을때 호출되는 콜백 </summary>
    private void OnCategoryToggleChanged(Toggles toggle, bool isOn)
    {
        if (!isOn)
        {
            return;
        }

        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.Button);

        SetCategoryActive(toggle);
    }
    
    /// <summary> InvenSlot의 마우스 오버 이벤트를 연결한다. </summary>
    private void BindInvenSlotEvents(InvenSlot slot)
    {
        slot.gameObject.BindEvent(UIEventType.PointerEnter, () => ShowItemDescription(slot));
        slot.gameObject.BindEvent(UIEventType.PointerExit, HideItemDescription);
    }
    
    /// <summary> InvenSlot의 모든 이벤트 구독을 해제시킨다. </summary>
    private void ClearInvenSlotEvents(InvenSlot slot)
    {
        slot.gameObject.ClearEvent();
    }
    
    /// <summary> Exit 버튼을 눌렀을 때 Popup을 닫는다. </summary>
    private void OnClickExit()
    {
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimeCancel);

        SetQuickBarActive(false);

        Close();
    }

    #endregion ===== 이벤트 =====
}