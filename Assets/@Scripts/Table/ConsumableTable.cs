using System;
using System.Collections.Generic;

[Serializable]
public class ConsumableData
{
    [CsvField("ID")]
    public int ID;

    [CsvField("아이템ID")]
    public string Name;

    [CsvField("HP")]
    public int Hp;

    [CsvField("SP")]
    public int SP;
}

public class ConsumableTable : TableLoader<ConsumableData>
{
    private readonly Dictionary<int, ConsumableData> _tableDatas = new();

    protected override string TableName => "ConsumableItem";

    protected override void AddData(ConsumableData data)
    {
        _tableDatas.Add(data.ID, data);
    }
}