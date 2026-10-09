using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TableData
{
    [CsvField("ID")]
    public int ID;
}

/// <summary> CSV 테이블 데이터를 읽어 TData로 파싱한다. (테이블 추가할때마다 해당 클래스를 상속) </summary>
public abstract class TableLoader<TData> where TData : TableData, new()
{
    private readonly TableParser<TData> _tableParser = new();

    protected readonly Dictionary<int, TData> TableMap = new();

    protected abstract string TableName { get; }

    public void Load()
    {
        TableMap.Clear();

        TextAsset textAsset = Managers.Resource.Load<TextAsset>(ResourceKey.Path.Table + TableName);

        if (textAsset == null)
        {
            return;
        }

        ParseTable(textAsset.text);

        OnLoaded();
    }

    /// <summary> 테이블 로딩 완료 후 추가 작업을 수행한다. </summary>
    protected virtual void OnLoaded()
    {
    }

    /// <summary> 로드한 테이블 데이터를 TData로 파싱한다. </summary>
    private void ParseTable(string text)
    {
        using StringReader stringReader = new StringReader(text);

        string headerLine = stringReader.ReadLine();

        if (string.IsNullOrEmpty(headerLine))
        {
            CPrint.Error($"[{TableName}] No header line found.");
            return;
        }

        CPrint.Log($"[{TableName}] LoadedHeader: {headerLine}");

        string line;

        while ((line = stringReader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            TData data = _tableParser.ParseLine(headerLine, line);

            if (data == null)
            {
                continue;
            }

            AddData(data);
        }
    }

    private void AddData(TData data)
    {
        TableMap.Add(data.ID, data);
    }
}