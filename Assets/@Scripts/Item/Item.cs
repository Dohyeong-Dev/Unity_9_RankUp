public enum ItemType
{
    None,
    Equipment,
    Consumable,
}

public enum BuffType
{
    None,
    HP,
    SP,
}

/// <summary> 게임에서 사용하는 아이템의 정의 데이터를 관리한다. </summary>
public class Item
{
    #region ===== 기본 정보 =====

    /// <summary> 아이템 ID </summary>
    public int ID { get; }

    /// <summary> 아이템 이름 </summary>
    public string Name { get; }

    /// <summary> 아이템 설명 </summary>
    public string Description { get; }

    /// <summary> 아이템 리소스 이름 </summary>
    public string ResourceName { get; }

    /// <summary> 아이템 종류 </summary>
    public ItemType Type { get; }

    /// <summary> 최대 보유 가능 개수 </summary>
    public int LimitCount { get; }

    #endregion

    #region ===== 버프 정보 =====

    /// <summary> 아이템 사용 시 적용되는 버프 종류 </summary>
    public BuffType BuffType { get; }

    /// <summary> 아이템 사용 시 적용되는 버프 수치 </summary>
    public int BuffValue { get; }

    #endregion

    /// <summary> CSV 테이블 데이터를 기반으로 아이템을 생성한다. </summary>
    public Item(int id, ItemData data)
    {
        ID = id;
        Name = data.Name;
        Description = string.Format(data.Description, data.BuffValue);
        ResourceName = data.ResourceName;
        Type = Utils.TryParseEnum(data.Type, out ItemType itemType) ? itemType : ItemType.None;
        LimitCount = data.LimitCount;

        BuffType = Utils.TryParseEnum(data.BuffType, out BuffType buffType) ? buffType : BuffType.None;
        BuffValue = data.BuffValue;
    }
}