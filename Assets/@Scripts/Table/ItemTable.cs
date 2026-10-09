using System.Collections.Generic;

public class ItemData : TableData
{
    [CsvField("이름")]
    public string Name;

    [CsvField("설명")]
    public string Description;

    [CsvField("버프타입")]
    public string BuffType;

    [CsvField("버프값")]
    public int BuffValue;

    [CsvField("리소스이름")]
    public string ResourceName;

    [CsvField("타입")]
    public string Type;

    [CsvField("개수제한")]
    public int LimitCount;
}

/// <summary> 아이템 테이블 데이터를 관리한다. </summary>
public class ItemTable : TableLoader<ItemData>
{
    private readonly Dictionary<int, Item> _itemMap = new();

    protected override string TableName => "ItemInfo";

    /// <summary> CSV 로딩 후 게임에서 사용할 아이템 객체를 생성한다. </summary>
    protected override void OnLoaded()
    {
        _itemMap.Clear();

        foreach (KeyValuePair<int, ItemData> pair in TableMap)
        {
            _itemMap.Add(pair.Key, new Item(pair.Key, pair.Value));
        }
    }

    /// <summary> 아이템 ID에 해당하는 아이템을 반환한다. </summary>
    public Item GetItem(int itemID)
    {
        if (_itemMap.TryGetValue(itemID, out Item item))
        {
            return item;
        }

        CPrint.Warning($"[ItemTable] ID({itemID}) not found!");
        return null;
    }
}