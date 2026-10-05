using System.Collections.Generic;

public class ShopData : TableData
{
    [CsvField("아이템ID")]
    public int ItemID;

    [CsvField("구매가격")]
    public int BuyPrice;

    [CsvField("판매가격")]
    public int SellPrice;

    [CsvField("판매중")]
    public bool IsSell;
}

public class ShopTable : TableLoader<ShopData>
{
    protected override string TableName => "Shop";

    private readonly Dictionary<ItemType, ShopData[]> _sellingItemList = new();

    /// <summary> 아이템 타입에 해당하는 현재 판매 중인 아이템 목록을 가져온다. </summary>
    public ShopData[] GetSellingItemList(ItemType itemType)
    {
        if (_sellingItemList.Count == 0)
        {
            InitializeSellingItemList();
        }

        return _sellingItemList.TryGetValue(itemType, out ShopData[] list) ? list : System.Array.Empty<ShopData>();
    }

    /// <summary> 현재 판매 중인 아이템을 타입별로 분류하여 초기화한다. </summary>
    private void InitializeSellingItemList()
    {
        Dictionary<ItemType, List<ShopData>> itemMap = new();

        foreach (ShopData data in DataMap.Values)
        {
            if (!data.IsSell)
            {
                continue;
            }

            ItemType itemType = Managers.Table.Item.GetItemType(data.ItemID);

            if (!itemMap.TryGetValue(itemType, out List<ShopData> itemList))
            {
                itemList = new List<ShopData>();
                itemMap.Add(itemType, itemList);
            }

            itemList.Add(data);
        }

        foreach (KeyValuePair<ItemType, List<ShopData>> pair in itemMap)
        {
            _sellingItemList.Add(pair.Key, pair.Value.ToArray());
        }
    }
}