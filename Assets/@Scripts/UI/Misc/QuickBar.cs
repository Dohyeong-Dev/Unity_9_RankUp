using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> QuickBar의 개별 슬롯에 표시되는 아이템 UI를 관리한다. </summary>
public class QuickSlot
{
    private readonly Image _clickImage;
    private readonly Image _itemImage;
    public Image ItemImage => _itemImage;
    private readonly Image _badgeImage;
    private readonly TMP_Text _itemCountText;

    public GameObject ClickObject => _clickImage.gameObject;

    public QuickSlot(Image clickImage, Image itemImage, Image badgeImage, TMP_Text itemCountText)
    {
        _clickImage = clickImage;
        _itemImage = itemImage;
        _badgeImage = badgeImage;
        _itemCountText = itemCountText;

        Clear();
    }

    /// <summary> QuickSlot에 아이템 정보를 표시한다. </summary>
    public void SetItem(Sprite sprite, int count)
    {
        if (sprite == null)
        {
            Clear();
            return;
        }

        _itemImage.sprite = sprite;
        _itemImage.gameObject.SetActive(true);

        bool hasCount = count > 0;

        _badgeImage.gameObject.SetActive(hasCount);
        _itemCountText.gameObject.SetActive(hasCount);

        if (hasCount)
        {
            _itemCountText.text = count.ToString("N0");
        }
    }

    /// <summary> QuickSlot의 아이템 정보를 초기화한다. </summary>
    public void Clear()
    {
        _itemImage.sprite = null;
        _itemImage.gameObject.SetActive(false);

        _badgeImage.gameObject.SetActive(false);

        _itemCountText.text = string.Empty;
        _itemCountText.gameObject.SetActive(false);
    }
}

/// <summary> QuickSlot 아이템을 표시하고 슬롯 클릭을 처리하는 QuickBar UI다. </summary>
public class QuickBar : BaseUI
{
    public enum Images
    {
        ClickImage,
        ClickImage1,
        ItemImage,
        ItemImage1,
        BadgeImage,
        BadgeImage1
    }

    public enum Texts
    {
        ItemCountText,
        ItemCountText1
    }

    private const int QuickSlotCount = 2;

    #region ===== 상태 =====

    private QuickSlot[] _quickSlots;

    #endregion ===== 상태 =====

    private void Awake()
    {
        Initialize();
    }

    #region ===== 초기화 =====

    /// <summary> QuickBar을 초기화한다. </summary>
    private void Initialize()
    {
        if (_quickSlots != null)
        {
            return;
        }
        
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));

        InitializeQuickSlots();
    }
    
    /// <summary> QuickSlot을 초기화한다. </summary>
    private void InitializeQuickSlots()
    {
        _quickSlots = new QuickSlot[QuickSlotCount];

        _quickSlots[0] = new QuickSlot(Get<Image>(Images.ClickImage),
            Get<Image>(Images.ItemImage), Get<Image>(Images.BadgeImage),
            Get<TMP_Text>(Texts.ItemCountText));

        _quickSlots[1] = new QuickSlot(Get<Image>(Images.ClickImage1),
            Get<Image>(Images.ItemImage1), Get<Image>(Images.BadgeImage1),
            Get<TMP_Text>(Texts.ItemCountText1));
    }

    #endregion ===== 초기화 =====

    /// <summary> 지정된 QuickSlot에 설정된 아이템 ID를 반환한다. </summary>
    public int GetItemID(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= QuickSlotCount)
        {
            return -1;
        }

        IReadOnlyList<InventoryItemData> inventoryItems = Managers.Data.GetInvenItemList();

        foreach (InventoryItemData itemData in inventoryItems)
        {
            if (itemData.QuickSlotIndex != slotIndex)
            {
                continue;
            }

            return itemData.ItemID;
        }

        return -1;
    }

    //
    public QuickSlot GetQuickSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= QuickSlotCount)
        {
            return null;
            
        }
        
        return _quickSlots[slotIndex];
    }
    
    /// <summary> QuickSlot 클릭 이벤트를 등록한다. </summary>
    public void SetSlotClickHandler(Action<int> handler)
    {
        if (handler == null)
        {
            return;
        }

        if (_quickSlots == null)
        {
            Initialize();
        }
        
        for (int i = 0; i < _quickSlots?.Length; i++)
        {
            int slotIndex = i;
            
            _quickSlots[i].ClickObject.ClearEvent();
            _quickSlots[i].ClickObject.BindEvent(UIEventType.Click, () => handler.Invoke(slotIndex));
        }
    }

    /// <summary> 저장된 QuickSlot 데이터를 UI에 반영한다. </summary>
    public void UpdateUI()
    {
        if (_quickSlots == null)
        {
            Initialize();
        }

        ClearAllSlots();

        IReadOnlyList<InventoryItemData> inventoryItems = Managers.Data.GetInvenItemList();

        foreach (InventoryItemData itemData in inventoryItems)
        {
            if (!IsValidSlotIndex(itemData.QuickSlotIndex))
            {
                continue;
            }

            Sprite sprite = GetItemSprite(itemData.ItemID);

            _quickSlots[itemData.QuickSlotIndex].SetItem(sprite, itemData.Count);
        }
    }

    /// <summary> QuickSlot 인덱스가 유효한지 확인한다. </summary>
    private bool IsValidSlotIndex(int slotIndex)
    {
        return _quickSlots != null && slotIndex >= 0 && slotIndex < _quickSlots.Length && _quickSlots[slotIndex] != null;
    }

    /// <summary> 모든 QuickSlot을 비운다. </summary>
    private void ClearAllSlots()
    {
        if (_quickSlots == null)
        {
            return;
        }

        for (int i = 0; i < _quickSlots.Length; i++)
        {
            if (_quickSlots[i] == null)
            {
                continue;
            }

            _quickSlots[i].Clear();
        }
    }

    /// <summary> 아이템 ID에 해당하는 Sprite를 반환한다. </summary>
    private Sprite GetItemSprite(int itemID)
    {
        string resourceName = Managers.Table.Item.GetItemResourceName(itemID);

        return Managers.Resource.Load<Sprite>(ResourceKey.Path.ItemSprite + resourceName);
    }
}