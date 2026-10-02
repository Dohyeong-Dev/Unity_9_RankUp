public class TableManager
{
    public EnemyTable Enemy { get; private set; }
    
    public ItemTable Item { get; private set; }
    public ConsumableTable Consumable { get; private set; }
    
    public ShopTable Shop { get; private set; }
    
    
    /// <summary> CSV 형태의 테이블들을 로드한다. </summary>
    public void Load()
    {
        // 적
        Enemy = new();
        Enemy.Load();
        
        // 아이템
        Item = new();
        Item.Load();
        Consumable = new();
        Consumable.Load();
        
        // 상점
        Shop = new();
        Shop.Load();
    }
}
