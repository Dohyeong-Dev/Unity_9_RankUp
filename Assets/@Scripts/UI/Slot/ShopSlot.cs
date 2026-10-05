using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : SlotUI
{
    public enum Images
    {
        ItemImage
    }

    private ItemData _itemData;
    private ShopData _shopData;
    
    private void Awake()
    {
        Bind<Image>(typeof(Images));
    }

    public void SetData(ShopData shopData)
    {
        _itemData = Managers.Table.Item.GetItemData(shopData.ItemID);
        _shopData = shopData;
    }
    
    public override void UpdateUI()
    {
        Get<Image>(Images.ItemImage).sprite = Managers.Resource.Load<Sprite>(ResourceKey.Path.ItemSprite +
                                                                             _itemData.ResourceName);
    }
}