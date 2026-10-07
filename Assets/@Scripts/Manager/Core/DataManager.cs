using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary> 게임의 로컬 세이브 데이터를 저장하고 관리한다. </summary>
public class DataManager
{
    private const string SaveFileName = "SaveData.json";

    private SaveData _saveData;

    public int Gold => _saveData?.Gold ?? 0;

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    /// <summary> 현재 세이브 데이터를 JSON 파일로 저장한다. </summary>
    public void Save()
    {
        string json = JsonUtility.ToJson(_saveData, true);
        CPrint.Log($"Save : {json}");
        
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
        CPrint.Log($"Load : {json}");

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

    #region ===== 골드 =====

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

    #endregion ===== 골드 =====

    #region ===== 인벤토리 =====

    /// <summary> 아이템을 구매하고 Gold와 인벤토리를 갱신한다. </summary>
    public PurchaseResult TryPurchaseItem(int itemID)
    {
        int price = Managers.Table.Shop.GetItemSellPrice(itemID);

        if (price < 0)
        {
            return PurchaseResult.InvalidItem;
        }

        if (!CanAddItem(itemID, 1))
        {
            return PurchaseResult.InventoryFull;
        }

        if (!TrySpendGold(price))
        {
            return PurchaseResult.NotEnoughGold;
        }

        TryAddItem(itemID, 1);
        Save();

        return PurchaseResult.Success;
    }
    
    /// <summary> 아이템의 현재 보유 개수를 반환한다. </summary>
    public int GetItemCount(int itemID)
    {
        InventoryItemData itemData = FindInventoryItem(itemID);

        return itemData?.Count ?? 0;
    }
    
    /// <summary> 인벤토리에서 아이템을 찾는다. </summary>
    private InventoryItemData FindInventoryItem(int itemID)
    {
        return _saveData.Inventory.Find(item => item.ItemID == itemID);
    }
    
    /// <summary> 지정된 개수의 아이템을 인벤토리에 추가할 수 있는지 확인한다. </summary>
    private bool CanAddItem(int itemID, int count)
    {
        if (count <= 0)
        {
            return false;
        }

        InventoryItemData inventoryItem = FindInventoryItem(itemID);

        if (inventoryItem == null)
        {
            return count <= Managers.Table.Item.GetItemLimitCount(itemID);
        }

        return inventoryItem.Count + count <= Managers.Table.Item.GetItemLimitCount(itemID);
    }
    
    /// <summary> 아이템을 지정된 개수만큼 추가한다. </summary>
    private bool TryAddItem(int itemID, int count)
    {
        if (count <= 0)
        {
            return false;
        }

        InventoryItemData inventoryItem = FindInventoryItem(itemID);

        if (inventoryItem == null)
        {
            _saveData.Inventory.Add(new InventoryItemData
            {
                ItemID = itemID,
                Count = count
            });

            return true;
        }

        if (inventoryItem.Count + count > Managers.Table.Item.GetItemLimitCount(itemID))
        {
            return false;
        }

        inventoryItem.Count += count;

        return true;
    }
    
    /// <summary> 현재 보유 중인 아이템 목록을 반환한다. </summary>
    public IReadOnlyList<InventoryItemData> GetInventoryItemList()
    {
        return _saveData.Inventory;
    }
    
    #endregion ===== 인벤토리 =====
}