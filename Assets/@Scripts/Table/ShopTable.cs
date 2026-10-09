using System.Collections.Generic;

public class ShopData : TableData
{
    [CsvField("아이템ID")]
    public int ItemID;

    [CsvField("판매가격")]
    public int SellPrice;

    [CsvField("판매중")]
    public bool IsSell;
}

public class ShopTable : TableLoader<ShopData>
{
    protected override string TableName => "Shop";

    private readonly Dictionary<ItemType, List<int>> _sellingItemIdList = new();

    /// <summary> 현재 판매 중인 아이템을 타입별로 분류하여 초기화한다. </summary>
    private void InitializeSellingItemList()
    {
        foreach (ShopData data in TableMap.Values)
        {
            if (!data.IsSell)
            {
                continue;
            }

            Item item = Managers.Table.Item.GetItem(data.ItemID);

            if (item == null)
            {
                return;
            }
            
            if (!_sellingItemIdList.TryGetValue(item.Type, out List<int> itemIdList))
            {
                itemIdList = new List<int>();
                _sellingItemIdList.Add(item.Type, itemIdList);
            }

            itemIdList.Add(data.ItemID);
        }
    }
    
    /// <summary> 아이템 타입에 해당하는 현재 판매 중인 아이템 목록을 가져온다. </summary>
    public IReadOnlyList<int> GetSellingItemIdList(ItemType itemType)
    {
        if (_sellingItemIdList.Count == 0)
        {
            InitializeSellingItemList();
        }

        if (_sellingItemIdList.ContainsKey(itemType))
        {
            return _sellingItemIdList[itemType];
        }

        return null;
    }

    /// <summary> 아이템ID에 해당하는 판매가격을 획득한다.  </summary>
    public int GetItemSellPrice(int itemID)
    {
        if (TableMap.ContainsKey(itemID))
        {
            return TableMap[itemID].SellPrice;
        }

        return 0;
    }
    
    /// <summary> 아이템ID에 해당하는 판매가격을 [판매가격 : ?]구조로 획득한다.  </summary>
    public string GetItemSellPriceStr(int itemID)
    {
        if (TableMap.ContainsKey(itemID))
        {
            return $"<br><color=yellow>판매가격 : {GetItemSellPrice(itemID):N0}</color>";
        }

        return string.Empty;
    }
}