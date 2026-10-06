using System.Collections.Generic;

public class ItemData : TableData
{
    [CsvField("이름")]
    public string Name;

    [CsvField("설명")]
    public string Description;
    
    [CsvField("버프값")]
    public string BuffValue;
    
    [CsvField("리소스이름")]
    public string ResourceName;
    
    [CsvField("타입")]
    public string Type;
    
    [CsvField("개수제한")]
    public int LimitCount;
}

public class ItemTable : TableLoader<ItemData>
{
    protected override string TableName => "ItemInfo";
    
    /// <summary> 아이템 ID에 해당하는 리소스이름을 가져온다. </summary>
    public string GetItemResourceName(int itemID)
    {
        if (DataMap.ContainsKey(itemID))
        {
            return DataMap[itemID].ResourceName;
        }

        return string.Empty;
    }
    
    /// <summary> 아이템 ID에 해당하는 타입을 획득한다. </summary>
    public ItemType GetItemType(int itemID)
    {
        if (DataMap.ContainsKey(itemID) && Utils.TryParseEnum(DataMap[itemID].Type, out ItemType type))
        {
            return type;
        }
        
        return ItemType.None;
    }

    /// <summary> 아이템 ID에 해당하는 이름을 획득한다. </summary>
    public string GetItemName(int itemID)
    {
        if (DataMap.ContainsKey(itemID))
        {
            return DataMap[itemID].Name;
        }
        
        return string.Empty;
    }
    
    /// <summary> 아이템 ID에 해당하는 설명에 버프 값을 적용하여 반환한다. </summary>
    public string GetItemDescription(int itemID)
    {
        if (DataMap.ContainsKey(itemID))
        {
            return string.Format(DataMap[itemID].Description, DataMap[itemID].BuffValue);
        }
        
        return string.Empty;
    }
}
