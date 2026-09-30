public class TableManager
{
    private readonly EnemyTable _enemy = new();
    public EnemyTable Enemy => _enemy;
    
    /// <summary> CSV 형태의 테이블들을 로드한다. </summary>
    public void Load()
    {
        _enemy.Load();
    }
}
