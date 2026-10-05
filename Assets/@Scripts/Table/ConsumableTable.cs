using System;
using System.Collections.Generic;

[Serializable]
public class ConsumableData : TableData
{
    [CsvField("아이템ID")]
    public string Name;

    [CsvField("HP")]
    public int Hp;

    [CsvField("SP")]
    public int SP;
}

public class ConsumableTable : TableLoader<ConsumableData>
{
    protected override string TableName => "ConsumableItem";
}