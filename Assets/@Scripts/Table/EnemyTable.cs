using System;
using System.Collections.Generic;

[Serializable]
public class EnemyData : TableData
{
    [CsvField("이름")]
    public string Name;

    [CsvField("최대HP")]
    public int MaxHp;

    [CsvField("경험치")]
    public int Exp;

    [CsvField("공격력")]
    public int Str;

    [CsvField("획득골드")]
    public int Gold;

    [CsvField("골드드랍확률")]
    public float GoldDropRate;
}

public class EnemyTable : TableLoader<EnemyData>
{
    protected override string TableName => "EnemyInfo";

    public EnemyData GetMonsterInfo(int monsterID)
    {
        return TableMap.GetValueOrDefault(monsterID);
    }
}