using System.Collections.Generic;

public class ItemData : TableData
{
    [CsvField("이름")]
    public string Name;

    [CsvField("설명")]
    public string Description;
    
    [CsvField("리소스이름")]
    public string ResourceName;

    [CsvField("타입")]
    public string Type;
}

public class ItemTable : TableLoader<ItemData>
{
    protected override string TableName => "ItemInfo";
    
    /// <summary> 아이템 ID에 해당하는 타입을 획득한다. </summary>
    public ItemType GetItemType(int itemID)
    {
        if (DataMap.ContainsKey(itemID) && Utils.TryParseEnum(DataMap[itemID].Type, out ItemType type))
        {
            CPrint.Log(type);
            return type;
        }
        
        return ItemType.None;
    }
}
