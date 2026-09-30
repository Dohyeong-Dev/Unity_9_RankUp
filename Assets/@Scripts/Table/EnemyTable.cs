using System;
using System.Collections.Generic;

[Serializable]
public class EnemyData
{
    [CsvField("몬스터ID")]
    public int ID;

    [CsvField("이름")]
    public string Name;

    [CsvField("최대HP")]
    public int MaxHp;

    [CsvField("경험치")]
    public int Exp;

    [CsvField("공격력")]
    public int Damage;

    [CsvField("코인드랍ID")]
    public int CoinDropId;

    [CsvField("코인드랍확률")]
    public float CoinDropRate;
}

public class EnemyTable : TableLoader<EnemyData>
{
    private readonly Dictionary<int, EnemyData> _tableDatas = new();

    protected override string TableName => "EnemyInfo";

    protected override void AddData(EnemyData data)
    {
        _tableDatas.Add(data.ID, data);
    }

    public EnemyData GetMonsterInfo(int monsterID)
    {
        return _tableDatas[monsterID];
    }
}