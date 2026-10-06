using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : SlotUI
{
    public enum Images
    {
        ItemImage
    }

    private int _itemID;
    public int ItemID => _itemID;

    private Button _button;

    private void Awake()
    {
        Bind<Image>(typeof(Images));

        _button = gameObject.GetOrAddComponent<Button>();
    }

    private void Start()
    {
        _button.onClick.AddListener(() => Managers.UI.OpenPopup<AlertPopup>().Set("구매하시겠습니까?",
            true, Buy));
    }

    public void SetData(int itemID)
    {
        _itemID = itemID;
    }

    public override void UpdateUI()
    {
        string resourceName = Managers.Table.Item.GetItemResourceName(_itemID);
        Get<Image>(Images.ItemImage).sprite = Managers.Resource.Load<Sprite>(ResourceKey.Path.ItemSprite +
                                                                             resourceName);
    }

    private void Buy()
    {
        CPrint.Log($"{_itemID} 구매");
    }
}