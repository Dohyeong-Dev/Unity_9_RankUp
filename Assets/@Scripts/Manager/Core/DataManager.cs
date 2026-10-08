using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary> 게임의 로컬 세이브 데이터를 저장하고 관리한다. </summary>
public class DataManager
{
    private const string SaveFileName = "SaveData.json";
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private SaveData _saveData;

    public event Action OnQuickSlotChanged;
    
    /// <summary> 현재 게임 데이터를 로컬 파일에 저장한다. </summary>
    public void Save()
    {
        string json = JsonUtility.ToJson(_saveData, true);

        CPrint.Log($"[DataManager] Save\n{json}");

        File.WriteAllText(SaveFilePath, json);
    }

    /// <summary> 로컬 파일에서 게임 데이터를 불러온다. </summary>
    public void Load()
    {
        if (!File.Exists(SaveFilePath))
        {
            _saveData = new SaveData();
            return;
        }

        string json = File.ReadAllText(SaveFilePath);

        CPrint.Log($"[DataManager] Load\n{json}");

        if (string.IsNullOrEmpty(json))
        {
            _saveData = new SaveData();
            return;
        }

        _saveData = JsonUtility.FromJson<SaveData>(json);

        if (_saveData == null)
        {
            _saveData = new SaveData();
        }

        InitializeInvenSlotIndex(ItemType.Equipment);
        InitializeInvenSlotIndex(ItemType.Consumable);
    }

    #region ===== 초기화 =====
   
    /// <summary> SaveData에 저장된 InvenSlotIndex가 믿을 만한 상태인지 검사하고 잘못된 슬롯 번호나 중복 슬롯이 있으면 빈 슬롯에 다시 배치한다. </summary>
    private void InitializeInvenSlotIndex(ItemType itemType)
    {
        List<InventoryItemData> itemList = GetInvenItems(itemType);

        int slotCount = GetInvenSlotCount(itemType);

        bool[] occupiedSlots = new bool[slotCount];

        foreach (InventoryItemData itemData in itemList)
        {
            int slotIndex = itemData.InvenSlotIndex;

            // 잘못된 슬롯 인덱스 또는 슬롯 인덱스가 슬롯의 개수를 넘는 경우
            if (slotIndex < 0 || slotIndex >= slotCount)
            {
                itemData.InvenSlotIndex = -1;
                continue;
            }

            // 슬롯 인덱스가 중복된 경우
            if (occupiedSlots[slotIndex])
            {
                itemData.InvenSlotIndex = -1;
                continue;
            }

            occupiedSlots[slotIndex] = true;
        }

        foreach (InventoryItemData itemData in itemList)
        {
            if (itemData.InvenSlotIndex >= 0)
            {
                continue;
            }

            // 잘못된 슬롯 인덱스들은 빈공간에 새로 배치한다.
            for (int i = 0; i < slotCount; i++)
            {
                if (occupiedSlots[i])
                {
                    continue;
                }

                itemData.InvenSlotIndex = i;
                occupiedSlots[i] = true;
                break;
            }
        }
    }

    #endregion ===== 초기화 =====
    
    #region ===== 골드 =====

    /// <summary> 골드를 추가한다. </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _saveData.Gold += amount;

        Save();
    }

    /// <summary> 골드를 차감할 수 있는지 확인하고 차감한다. </summary>
    private bool TrySpendGold(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        if (_saveData.Gold < amount)
        {
            return false;
        }

        _saveData.Gold -= amount;

        return true;
    }

    /// <summary> 현재 소지금을 가져온다. </summary>
    public int GetCurrentGold()
    {
        return _saveData?.Gold ?? 0;
    }
    
    #endregion ===== 골드 =====

    #region ===== 인벤토리 =====

    /// <summary> 아이템을 인벤토리에 추가한다. </summary>
    private bool TryAddItem(int itemID, int count)
    {
        if (!CanAddItem(itemID, count))
        {
            return false;
        }

        InventoryItemData inventoryItem = GetInventoryItem(itemID);

        // 이미 소지 중인 경우 수량을 추가한다.
        if (inventoryItem != null)
        {
            inventoryItem.Count += count;

            OnQuickSlotChanged?.Invoke();
            Save();

            return true;
        }

        // 새로운 아이템인 경우 빈 슬롯에 추가한다.
        ItemType itemType = Managers.Table.Item.GetItemType(itemID);
        int emptySlotIndex = GetEmptyInvenSlotIndex(itemType);

        inventoryItem = new InventoryItemData
        {
            ItemID = itemID,
            Count = count,
            InvenSlotIndex = emptySlotIndex,
            QuickSlotIndex = -1
        };

        _saveData.Inventory.Add(inventoryItem);

        OnQuickSlotChanged?.Invoke();
        Save();

        return true;
    }

    /// <summary> 지정된 아이템을 사용할 수 있는지 확인하고 수량을 차감한다. </summary>
    public bool TryUseItem(int itemID)
    {
        if (itemID <= 0)
        {
            return false;
        }

        InventoryItemData item = GetInventoryItem(itemID);

        if (item == null || item.Count <= 0)
        {
            return false;
        }

        if (Managers.Table.Item.GetItemType(itemID) != ItemType.Consumable)
        {
            return false;
        }

        item.Count--;

        // 모두 사용한 경우 인벤토리에서 제거한다.
        if (item.Count <= 0)
        {
            _saveData.Inventory.Remove(item);
        }
        
        OnQuickSlotChanged?.Invoke();
        Save();

        return true;
    }
    
    /// <summary> 아이템을 인벤토리에 추가할 수 있는지 확인한다. </summary>
    private bool CanAddItem(int itemID, int count)
    {
        if (count <= 0)
        {
            return false;
        }

        int limitCount = Managers.Table.Item.GetItemLimitCount(itemID);

        if (limitCount <= 0)
        {
            return false;
        }

        InventoryItemData inventoryItem = GetInventoryItem(itemID);

        if (inventoryItem != null)
        {
            return inventoryItem.Count + count <= limitCount;
        }

        ItemType itemType = Managers.Table.Item.GetItemType(itemID);

        return GetEmptyInvenSlotIndex(itemType) >= 0;
    }

    /// <summary> 지정된 아이템을 인벤토리의 다른 슬롯으로 이동한다. </summary>
    public bool TryMoveInvenIndex(int itemID, ItemType itemType, int targetSlotIndex)
    {
        if (itemID <= 0)
        {
            return false;
        }

        if (targetSlotIndex < 0)
        {
            return false;
        }

        InventoryItemData sourceItem = GetInventoryItem(itemID);

        if (sourceItem == null)
        {
            return false;
        }

        if (Managers.Table.Item.GetItemType(sourceItem.ItemID) != itemType)
        {
            return false;
        }

        InventoryItemData targetItem = GetInventoryItem(itemType, targetSlotIndex);

        // 같은 슬롯을 다시 클릭한 경우
        if (sourceItem == targetItem)
        {
            return false;
        }

        // 대상 슬롯에 아이템이 있으면 두 아이템의 위치를 교환한다.
        if (targetItem != null)
        {
            (sourceItem.InvenSlotIndex, targetItem.InvenSlotIndex) = (targetItem.InvenSlotIndex, sourceItem.InvenSlotIndex);
        }
        // 대상 슬롯이 비어 있으면 선택한 아이템만 이동한다.
        else
        {
            sourceItem.InvenSlotIndex = targetSlotIndex;
        }

        Save();

        return true;
    }

    /// <summary> 지정된 아이템을 QuickSlot에 등록한다. </summary>
    public bool TrySetQuickSlot(int itemID, int targetQuickSlotIndex)
    {
        if (itemID <= 0 || targetQuickSlotIndex < 0)
        {
            return false;
        }

        InventoryItemData sourceItem = GetInventoryItem(itemID);
        if (sourceItem == null)
        {
            return false;
        }

        ItemType itemType = Managers.Table.Item.GetItemType(itemID);
        if (itemType != ItemType.Consumable)
        {
            return false;
        }

        InventoryItemData targetItem = GetQuickSlotItem(targetQuickSlotIndex);
        // 같은 QuickSlot을 다시 클릭한 경우
        if (sourceItem == targetItem)
        {
            return true;
        }

        // 이미 아이템이 있으면 두 QuickSlot 위치를 교환한다.
        if (targetItem != null)
        {
            (sourceItem.QuickSlotIndex, targetItem.QuickSlotIndex) = (targetItem.QuickSlotIndex, sourceItem.QuickSlotIndex);
        }
        else // 빈 QuickSlot이면 해당 위치에 등록한다.
        {
            sourceItem.QuickSlotIndex = targetQuickSlotIndex;
        }

        Save();
        OnQuickSlotChanged?.Invoke();

        return true;
    }

    public void ClearQuickSlot(int quickSlotIndex)
    {
        if (quickSlotIndex < 0)
        {
            return;
        }
        
        InventoryItemData item = GetQuickSlotItem(quickSlotIndex);
        if (item != null)
        {
            item.QuickSlotIndex = -1;
            
            Save();
            OnQuickSlotChanged?.Invoke();
        }
    }

    /// <summary> 지정된 타입의 인벤토리 아이템 목록을 반환한다. </summary>
    public List<InventoryItemData> GetInvenItems(ItemType itemType)
    {
        List<InventoryItemData> itemList = new();

        foreach (InventoryItemData itemData in _saveData.Inventory)
        {
            if (Managers.Table.Item.GetItemType(itemData.ItemID) != itemType)
            {
                continue;
            }

            itemList.Add(itemData);
        }

        return itemList;
    }

    /// <summary> 현재 인벤토리의 전체 아이템 목록을 반환한다. </summary>
    public IReadOnlyList<InventoryItemData> GetInvenItemList()
    {
        return _saveData.Inventory;
    }
    
    /// <summary> 아이템 타입에 따른 인벤토리 슬롯 개수를 반환한다. </summary>
    public int GetInvenSlotCount(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Equipment => 20,
            ItemType.Consumable => 32,
            _ => 0
        };
    }

    /// <summary> 지정된 타입의 비어있는 인벤토리 슬롯을 반환한다. </summary>
    private int GetEmptyInvenSlotIndex(ItemType itemType)
    {
        int slotCount = GetInvenSlotCount(itemType);

        if (slotCount <= 0)
        {
            return -1;
        }

        List<InventoryItemData> itemList = GetInvenItems(itemType);

        for (int i = 0; i < slotCount; i++)
        {
            bool isOccupied = false;

            foreach (InventoryItemData itemData in itemList)
            {
                if (itemData.InvenSlotIndex != i)
                {
                    continue;
                }

                isOccupied = true;
                break;
            }

            if (!isOccupied)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary> 아이템 ID에 해당하는 인벤토리 데이터를 반환한다. </summary>
    private InventoryItemData GetInventoryItem(int itemID)
    {
        foreach (InventoryItemData itemData in _saveData.Inventory)
        {
            if (itemData.ItemID == itemID)
            {
                return itemData;
            }
        }

        return null;
    }

    /// <summary> 타입과 슬롯 인덱스가 일치하는 인벤토리 데이터를 반환한다. </summary>
    private InventoryItemData GetInventoryItem(ItemType itemType, int slotIndex)
    {
        foreach (InventoryItemData itemData in _saveData.Inventory)
        {
            if (Managers.Table.Item.GetItemType(itemData.ItemID) != itemType)
            {
                continue;
            }

            if (itemData.InvenSlotIndex == slotIndex)
            {
                return itemData;
            }
        }

        return null;
    }

    /// <summary> 지정된 QuickSlot에 등록된 아이템을 반환한다. </summary>
    private InventoryItemData GetQuickSlotItem(int quickSlotIndex)
    {
        foreach (InventoryItemData itemData in _saveData.Inventory)
        {
            if (itemData.QuickSlotIndex == quickSlotIndex)
            {
                return itemData;
            }
        }

        return null;
    }
    
    #endregion ===== 인벤토리 =====

    #region ===== 상점 =====

    /// <summary> 아이템 구매를 시도하고 결과를 반환한다. </summary>
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

        if (!TryAddItem(itemID, 1))
        {
            return PurchaseResult.InventoryFull;
        }

        Save();

        return PurchaseResult.Success;
    }

    #endregion ===== 상점 =====
}