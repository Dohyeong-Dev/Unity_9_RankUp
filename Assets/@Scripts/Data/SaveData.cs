using System;
using System.Collections.Generic;

/// <summary> 게임의 로컬 세이브 데이터를 정의한다. </summary>
[Serializable]
public class SaveData
{
    public int Gold;
    public List<InventoryItemData> Inventory = new();
}

/// <summary>  인벤토리에 저장되는 아이템 정보를 정의한다. </summary>
[Serializable]
public class InventoryItemData
{
    public int ItemID;
    public int Count;
}