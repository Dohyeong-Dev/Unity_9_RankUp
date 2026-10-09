using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 인벤토리의 아이템 목록과 카테고리 전환 및 아이템 선택을 관리하는 Popup UI다. </summary>
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

    #region ===== 상태 =====

    private InvenSlot[] _equipmentSlots;
    private InvenSlot[] _consumableSlots;

    private InvenSlot _selectedSlot;
    private InvenSlot _hoveredSlot;
    private int _selectedQuickSlotIndex = -1;

    private Image _selectedTempImage;

    #endregion ===== 상태 =====

    #region ===== 참조 =====

    private ItemDescription _itemDescription;
    private QuickBar _quickBar;

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
        CreateInvenSlots();
        CreateSelectedTempImage();

        InitializeExitButton();
        InitializeCategory();

        // 퀵바의 슬롯 클릭 이벤트 연결
        if (_quickBar != null)
        {
            _quickBar.SetSlotClickHandler(OnClickQuickSlot);
        }

        // 인벤토리 팝업 클릭 이벤트 연결
        gameObject.BindEvent(UIEventType.Click, OnPointerClick);

        UpdateInvenUI();
    }

    protected override void OnUpdate()
    {
        UpdateSelectedTempImagePos();
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
        CancelSelectedItem();

        SetQuickBarActive(false);

        if (Managers.Scene != null && Managers.Scene.TryGetCurrentScene(out GameScene scene))
        {
            scene.HUD.SetSideIconActive(GameHUD.SideBar.Inventory, false);
        }
    }

    #region ===== 초기화 =====

    /// <summary> ItemDescription을 찾아 참조한다. </summary>
    private void ResolveItemDescription()
    {
        _itemDescription = GetComponentInChildren<ItemDescription>();

        if (_itemDescription == null)
        {
            CPrint.Warning("[InvenPopup] ItemDescription을 찾을 수 없습니다.");
        }
    }

    /// <summary> QuickBar를 찾아 참조한다. </summary>
    private void ResolveQuickBar()
    {
        _quickBar = gameObject.FindChild<QuickBar>("QuickBar");

        if (_quickBar == null)
        {
            CPrint.Warning("[InvenPopup] QuickBar를 찾을 수 없습니다.");
            return;
        }

        SetQuickBarActive(false);
    }

    /// <summary> 마우스를 따라다닐 선택된 아이템 임시 이미지를 생성한다. </summary>
    private void CreateSelectedTempImage()
    {
        GameObject imageObject = new GameObject("SelectedTempImage");
        imageObject.transform.SetParent(transform, false);
        imageObject.transform.SetAsLastSibling();

        _selectedTempImage = imageObject.AddComponent<Image>();

        _selectedTempImage.raycastTarget = false;
        _selectedTempImage.preserveAspect = true;

        HideSelectedTempImage();
    }

    /// <summary> 인벤토리 슬롯들을 생성한다. </summary>
    private void CreateInvenSlots()
    {
        CreateInvenSlotsByType(ItemType.Equipment, Get<ScrollRect>(Scrolls.EquipmentScroll));
        CreateInvenSlotsByType(ItemType.Consumable, Get<ScrollRect>(Scrolls.ConsumableScroll));
    }

    /// <summary> 지정된 타입의 인벤토리 슬롯들을 생성한다. </summary>
    private void CreateInvenSlotsByType(ItemType itemType, ScrollRect scroll)
    {
        InvenSlot[] slotList = itemType switch
        {
            ItemType.Equipment => _equipmentSlots = new InvenSlot[Managers.Data.GetInvenSlotCount(itemType)],
            ItemType.Consumable => _consumableSlots = new InvenSlot[Managers.Data.GetInvenSlotCount(itemType)],
            _ => null
        };

        if (slotList == null)
        {
            return;
        }

        for (int i = 0; i < slotList.Length; i++)
        {
            int slotIndex = i;

            InvenSlot slot = Managers.UI.MakeSlot<InvenSlot>(scroll.content);

            if (slot == null)
            {
                CPrint.Error($"[InvenPopup] InvenSlot 생성 실패. Index: {slotIndex}");
                continue;
            }

            slotList[i] = slot;
            slotList[i].SetSlotInfo(itemType, slotIndex);
        }
    }

    /// <summary> Exit 버튼 이벤트를 초기화한다. </summary>
    private void InitializeExitButton()
    {
        Get<Button>(Buttons.ExitButton).onClick.AddListener(OnClickExit);
    }

    /// <summary> 인벤토리 카테고리의 초기 상태를 설정한다. </summary>
    private void InitializeCategory()
    {
        Get<Toggle>(Toggles.EquipmentToggle).onValueChanged.AddListener(isOn =>
            OnCategoryChanged(Toggles.EquipmentToggle, isOn));

        Get<Toggle>(Toggles.ConsumableToggle).onValueChanged.AddListener(isOn =>
            OnCategoryChanged(Toggles.ConsumableToggle, isOn));

        ActivateScroll(Toggles.EquipmentToggle);
    }

    #endregion ===== 초기화 =====

    #region ===== 갱신 =====

    /// <summary> 인벤토리와 QuickBar를 갱신한다. </summary>
    public void UpdateInvenUI()
    {
        UpdateInvenSlots(ItemType.Equipment);
        UpdateInvenSlots(ItemType.Consumable);

        UpdateGoldText();
        UpdateQuickBar();

        RefreshItemDescription();
    }

    /// <summary> 지정된 타입의 인벤토리 슬롯을 갱신한다. </summary>
    private void UpdateInvenSlots(ItemType itemType)
    {
        InvenSlot[] slotList = GetSlotList(itemType);

        if (slotList == null)
        {
            return;
        }

        List<InventoryItemData> inventoryItems = Managers.Data.GetInvenItemDataList(itemType);

        ClearInvenSlots(slotList);

        foreach (InventoryItemData itemData in inventoryItems)
        {
            int slotIndex = itemData.InvenSlotIndex;

            if (slotIndex < 0 || slotIndex >= slotList.Length)
            {
                continue;
            }

            InvenSlot slot = slotList[slotIndex];

            slot.SetData(itemData.ItemID, itemData.Count);

            BindInvenSlotEvents(slot);

            slot.UpdateUI();
        }
    }

    /// <summary> 지정된 타입의 모든 인벤토리 슬롯을 초기화한다. </summary>
    private void ClearInvenSlots(InvenSlot[] slotList)
    {
        for (int i = 0; i < slotList.Length; i++)
        {
            InvenSlot slot = slotList[i];

            if (slot == null)
            {
                continue;
            }

            slot.ClearData();
            BindInvenSlotEvents(slot);

            slot.UpdateUI();
        }
    }

    /// <summary> 현재 보유 Gold를 UI에 표시한다. </summary>
    private void UpdateGoldText()
    {
        Get<TMP_Text>(Texts.GoldText).text = Managers.Data.GetCurrentGold().ToString("N0");
    }

    /// <summary> 저장된 QuickSlot 데이터를 InvenPopup의 QuickBar에 반영한다. </summary>
    private void UpdateQuickBar()
    {
        _quickBar?.UpdateUI();
    }

    /// <summary> 선택된 아이템 임시 이미지의 위치를 마우스 위치로 갱신한다. </summary>
    private void UpdateSelectedTempImagePos()
    {
        if (_selectedTempImage.sprite == null || !_selectedTempImage.gameObject.activeSelf)
        {
            return;
        }

        _selectedTempImage.transform.position = Input.mousePosition;
    }

    #endregion ===== 갱신 =====

    #region ===== 활성화/비활성화 =====

    /// <summary> 선택된 아이템 임시 이미지를 숨긴다. </summary>
    private void HideSelectedTempImage()
    {
        if (_selectedTempImage == null)
        {
            return;
        }

        _selectedTempImage.sprite = null;
        _selectedTempImage.gameObject.SetActive(false);
    }

    /// <summary> QuickBar의 활성화 상태를 설정한다. </summary>
    private void SetQuickBarActive(bool isActive)
    {
        if (_quickBar == null)
        {
            return;
        }

        _quickBar.gameObject.SetActive(isActive);
    }

    /// <summary> 지정된 인벤토리 카테고리의 스크롤을 활성화한다. </summary>
    private void ActivateScroll(Toggles toggle)
    {
        HideAllInventoryScrolls();

        Scrolls scrollType = toggle switch
        {
            Toggles.EquipmentToggle => Scrolls.EquipmentScroll,
            Toggles.ConsumableToggle => Scrolls.ConsumableScroll,
            _ => Scrolls.EquipmentScroll
        };

        Get<ScrollRect>(scrollType).gameObject.SetActive(true);

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

    #endregion ===== 활성화/비활성화 =====

    #region ===== 아이템 선택 =====

    /// <summary> 마우스를 따라다니는 임시 이미지를 선택한 아이템의 이미지로 바꾼 후 활성화시킨다. </summary>
    private void ChangeSelectedTempImage(Image selectedItemImage)
    {
        _selectedTempImage.sprite = selectedItemImage.sprite;

        RectTransform sourceRect = selectedItemImage.rectTransform;
        RectTransform tempRect = _selectedTempImage.rectTransform;

        tempRect.sizeDelta = sourceRect.rect.size;
        tempRect.pivot = sourceRect.pivot;

        _selectedTempImage.gameObject.SetActive(true);

        UpdateSelectedTempImagePos();
    }

    /// <summary> 선택된 아이템을 지정된 인벤토리 슬롯으로 이동한다. </summary>
    private void PlaceSelectedItem(InvenSlot targetSlot)
    {
        if (_selectedSlot == null || targetSlot == null)
        {
            return;
        }

        // 슬롯 타입이 다른 경우
        if (_selectedSlot.Type != targetSlot.Type)
        {
            return;
        }

        if (Managers.Data.TryMoveInvenIndex(_selectedSlot.ItemID, targetSlot.Type, targetSlot.Index))
        {
            UpdateInvenUI();
        }

        CancelSelectedItem();
    }

    /// <summary> 선택된 아이템을 지정된 QuickBar 슬롯에 배치한다. </summary>
    private void PlaceSelectedItemInQuickSlot(int quickSlotIndex)
    {
        if (_selectedSlot == null)
        {
            return;
        }

        if (_selectedSlot.Type != ItemType.Consumable)
        {
            return;
        }

        if (Managers.Data.TrySetQuickSlot(_selectedSlot.ItemID, quickSlotIndex))
        {
            CancelSelectedItem();
            UpdateInvenUI();
        }
    }

    /// <summary> 현재 선택된 아이템을 취소한다. </summary>
    private void CancelSelectedItem()
    {
        _selectedSlot = null;
        SetSelectedQuickSlotIndex(-1);
        
        HideSelectedTempImage();
    }

    #endregion ===== 아이템 선택 =====

    #region ===== 아이템 설명 =====

    /// <summary> 지정된 슬롯의 아이템 설명을 표시한다. </summary>
    private void ShowItemDescription(InvenSlot slot)
    {
        if (_itemDescription == null || slot == null || !slot.HasItem)
        {
            return;
        }

        _hoveredSlot = slot;

        _itemDescription.Show(slot);
    }

    /// <summary> 아이템 설명을 숨긴다. </summary>
    private void HideItemDescription()
    {
        if (_itemDescription == null)
        {
            return;
        }

        _hoveredSlot = null;

        _itemDescription.Hide();
    }

    /// <summary> 현재 선택된 슬롯의 아이템 설명을 갱신한다. </summary>
    private void RefreshItemDescription()
    {
        if (_hoveredSlot == null)
        {
            return;
        }

        if (!_hoveredSlot.HasItem)
        {
            HideItemDescription();
            return;
        }

        _itemDescription.Show(_hoveredSlot);
    }

    #endregion ===== 아이템 설명 =====

    #region ===== 퀵바 =====
    
    /// <summary> QuickBar의 아이템을 선택하고 마우스 이동용 이미지로 설정한다. </summary>
    private void SelectQuickSlot(int slotIndex)
    {
        int itemID = _quickBar.GetItemID(slotIndex);

        if (itemID < 0)
        {
            return;
        }

        SetSelectedQuickSlotIndex(slotIndex);

        Image image = _quickBar.GetQuickSlot(slotIndex).ItemImage;

        if (image == null)
        {
            CancelSelectedItem();
            return;
        }

        ChangeSelectedTempImage(image);
    }
    
    /// <summary> 선택한 QuickSlot 아이템을 지정된 QuickSlot으로 이동한다. </summary>
    private void MoveSelectedQuickSlot(int targetSlotIndex)
    {
        if (_selectedQuickSlotIndex == targetSlotIndex)
        {
            CancelSelectedItem();
            return;
        }

        int itemID = _quickBar.GetItemID(_selectedQuickSlotIndex);

        if (itemID < 0)
        {
            CancelSelectedItem();
            return;
        }

        if (Managers.Data.TrySetQuickSlot(itemID, targetSlotIndex))
        {
            CancelSelectedItem();
            UpdateInvenUI();
        }
    }

    private void SetSelectedQuickSlotIndex(int slotIndex)
    {
        _selectedQuickSlotIndex = slotIndex;
    }
    
    /// <summary> 선택된 QuickSlot 아이템을 QuickBar에서 해제한다. </summary>
    private void RemoveSelectedQuickSlot()
    {
        if (_selectedQuickSlotIndex < 0)
        {
            return;
        }

        Managers.Data.UnassignQuickSlot(_selectedQuickSlotIndex);
        UpdateQuickBar();
        
        CancelSelectedItem();
    }

    #endregion ===== 퀵바 =====
    
    #region ===== 이벤트 =====

    /// <summary> Popup의 슬롯 외 영역 클릭을 처리한다. </summary>
    public void OnPointerClick()
    {
        if (_selectedQuickSlotIndex < 0)
        {
            CancelSelectedItem();
        }
        else // 퀵슬롯을 선택한상태로 퀵슬롯영역이 아닌 곳에 클릭한 경우 해제한다.
        {
            RemoveSelectedQuickSlot();
        }
    }

    /// <summary> 해당 카테고리가 활성화됐을 때 호출된다. </summary>
    private void OnCategoryChanged(Toggles toggle, bool isOn)
    {
        if (!isOn)
        {
            return;
        }

        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.Button);

        CancelSelectedItem();
        ActivateScroll(toggle);
    }

    /// <summary> 인벤토리 슬롯의 마우스 오버 및 클릭 이벤트를 연결한다. </summary>
    private void BindInvenSlotEvents(InvenSlot slot)
    {
        if (slot == null)
        {
            return;
        }

        slot.gameObject.ClearEvent();
        slot.gameObject.BindEvent(UIEventType.PointerEnter, () => ShowItemDescription(slot));
        slot.gameObject.BindEvent(UIEventType.PointerExit, HideItemDescription);
        slot.gameObject.BindEvent(UIEventType.Click, () =>
        {
            RemoveSelectedQuickSlot();
            OnClickInvenSlot(slot);
        });
    }

    /// <summary> 인벤토리 슬롯 클릭을 처리한다. </summary>
    private void OnClickInvenSlot(InvenSlot slot)
    {
        if (slot == null)
        {
            return;
        }

        if (_selectedSlot == null) // 첫 클릭
        {
            if (!slot.HasItem)
            {
                return;
            }

            _selectedSlot = slot;
            ChangeSelectedTempImage(_selectedSlot.GetItemImage());
        }
        else // 아이템 임시 이미지 생성 상태
        {
            if (_selectedSlot == slot) // 선택한 슬롯을 다시 클릭한 경우
            {
                CancelSelectedItem();
            }
            else // 선택한 슬롯외의 슬롯을 클릭한 경우
            {
                PlaceSelectedItem(slot);
            }
        }
    }

    /// <summary> QuickBar 슬롯 클릭을 처리한다. </summary>
    private void OnClickQuickSlot(int slotIndex)
    {
        // 인벤토리 아이템을 선택한 상태라면 QuickBar에 배치한다.
        if (_selectedSlot != null)
        {
            PlaceSelectedItemInQuickSlot(slotIndex);
            return;
        }

        // 이미 QuickBar 아이템을 선택한 상태라면 QuickSlot끼리 위치를 교환한다.
        if (_selectedQuickSlotIndex >= 0)
        {
            MoveSelectedQuickSlot(slotIndex);
            return;
        }

        SelectQuickSlot(slotIndex);
    }

    /// <summary> Exit 버튼을 눌렀을 때 Popup을 닫는다. </summary>
    private void OnClickExit()
    {
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimeCancel);

        CancelSelectedItem();
        SetQuickBarActive(false);

        Close();
    }

    #endregion ===== 이벤트 =====

    #region ===== Get =====

    /// <summary> 지정된 타입의 인벤토리 슬롯 배열을 반환한다. </summary>
    private InvenSlot[] GetSlotList(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Equipment => _equipmentSlots,
            ItemType.Consumable => _consumableSlots,
            _ => null
        };
    }

    #endregion ===== Get =====
}