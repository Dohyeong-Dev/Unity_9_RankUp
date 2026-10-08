using System;
using System.Collections.Generic;

/// <summary> 게임의 로컬 세이브 데이터를 정의한다. </summary>
[Serializable]
public class SaveData
{
    public int Gold;
    public List<InventoryItemData> Inventory = new();
}

/// <summary> 인벤토리에 저장되는 개별 아이템 정보를 관리한다. </summary>
[Serializable]
public class InventoryItemData
{
    public int ItemID;
    public int Count;

    /// <summary> 인벤토리에서 아이템이 위치한 슬롯 인덱스다. </summary>
    public int InvenSlotIndex = -1;

    /// <summary> QuickSlot에 등록된 슬롯 인덱스다. </summary>
    public int QuickSlotIndex = -1;
}