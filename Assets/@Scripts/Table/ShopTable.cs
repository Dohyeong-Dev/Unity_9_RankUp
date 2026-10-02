public class ShopData
{
    [CsvField("ID")]
    public int ID;

    [CsvField("아이템ID")]
    public string ItemID;

    [CsvField("구매가격")]
    public string BuyPrice;
    
    [CsvField("판매가격")]
    public int SellPrice;

    [CsvField("판매중")]
    public bool IsSell;
}

public class ShopTable : TableLoader<ShopData>
{
    protected override string TableName => "Shop";
    
    protected override void AddData(ShopData data)
    {
    }
}