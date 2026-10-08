using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 인벤토리에서 아이템 하나의 정보를 표시하는 슬롯 UI다. </summary>
public class InvenSlot : SlotUI
{
    public enum Images
    {
        ItemImage,
        BadgeImage,
    }

    public enum Texts
    {
        ItemCountText
    }
    
    #region ===== 상태 =====

    // 아이템 정보
    private int _itemID;
    public int ItemID => _itemID;
    public bool HasItem => _itemID > 0 && _count > 0;
    private int _count;
    public int Count => _count;
    
    // 슬롯 정보
    private int _slotIndex;
    public int Index => _slotIndex;
    private ItemType _itemType;
    public ItemType Type => _itemType;

    #endregion ===== 상태 =====

    private void Awake()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
    }

    #region ===== 데이터 =====

    /// <summary> 인벤토리 슬롯 정보 설정한다. </summary>
    public void SetSlotInfo(ItemType itemType, int slotIndex)
    {
        _itemType = itemType;
        _slotIndex = slotIndex;
    }
    
    /// <summary> 인벤토리 아이템 정보를 슬롯에 설정한다. </summary>
    public void SetData(int itemID, int count)
    {
        _itemID = itemID;
        _count = count;
    }

    /// <summary> 슬롯의 아이템 데이터를 초기화한다. </summary>
    public void ClearData()
    {
        _itemID = 0;
        _count = 0;
    }

    #endregion ===== 데이터 =====

    #region ===== UI =====

    /// <summary> 현재 슬롯 데이터를 UI에 반영한다. </summary>
    public override void UpdateUI()
    {
        Image itemImage = Get<Image>(Images.ItemImage);
        Image badgeImage = Get<Image>(Images.BadgeImage);
        TMP_Text itemText = Get<TMP_Text>(Texts.ItemCountText);
        
        if (!HasItem)
        {
            ClearData();
            
            itemImage.sprite = null;
            itemImage.gameObject.SetActive(false);
            badgeImage.gameObject.SetActive(false);
            
            return;
        }

        itemImage.sprite = GetSprite();
        itemImage.gameObject.SetActive(true);
        badgeImage.gameObject.SetActive(true);
        itemText.text = Count.ToString("N0");
    }

    /// <summary> 현재 아이템 이미지의 RectTransform을 반환한다. </summary>
    public Image GetItemImage()
    {
        return Get<Image>(Images.ItemImage);
    }
    
    /// <summary> 현재 아이템의 Sprite를 반환한다. </summary>
    private Sprite GetSprite()
    {
        if (!HasItem)
        {
            return null;
        }

        string resourceName = Managers.Table.Item.GetItemResourceName(_itemID);

        return Managers.Resource.Load<Sprite>(ResourceKey.Path.ItemSprite + resourceName);
    }

    #endregion ===== UI =====
}