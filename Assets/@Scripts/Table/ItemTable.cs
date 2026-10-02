public class ItemData
{
    [CsvField("ID")]
    public int ID;

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
    
    protected override void AddData(ItemData data)
    {
    }
}
