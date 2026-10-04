using System.IO;
using UnityEngine;

/// <summary> 게임의 로컬 세이브 데이터를 저장하고 관리한다. </summary>
public class DataManager
{
    private const string SaveFileName = "SaveData.json";

    private SaveData _saveData;

    public int Gold => _saveData?.Gold ?? 0;

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    /// <summary> Gold를 지정된 수량만큼 증가시킨다. </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _saveData.Gold += amount;
    }

    /// <summary> Gold를 지정된 수량만큼 감소시킨다. </summary>
    public bool TrySpendGold(int amount)
    {
        if (amount <= 0 || _saveData.Gold < amount)
        {
            return false;
        }

        _saveData.Gold -= amount;
        return true;
    }

    /// <summary> 현재 세이브 데이터를 JSON 파일로 저장한다. </summary>
    public void Save()
    {
        string json = JsonUtility.ToJson(_saveData, true);
        File.WriteAllText(SaveFilePath, json);
    }

    /// <summary> 저장된 세이브 데이터를 불러온다. </summary>
    public void Load()
    {
        if (!File.Exists(SaveFilePath))
        {
            _saveData = new SaveData();
            return;
        }

        // 파일 로드
        string json = File.ReadAllText(SaveFilePath);

        if (string.IsNullOrEmpty(json))
        {
            _saveData = new SaveData();
            return;
        }

        // 파싱
        _saveData = JsonUtility.FromJson<SaveData>(json);

        if (_saveData == null)
        {
            _saveData = new SaveData();
        }
    }
}