using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리에 보유 중인 아이템의 정보를 표시하는 슬롯 UI다.
/// </summary>
public class InvenSlot : SlotUI
{
    public enum Images
    {
        ItemImage,
        BadgeImage,
    }

    public enum Texts
    {
        ItemCountText
    }

    #region ===== 상태 =====

    private int _itemID;
    public int ItemID => _itemID;
    private int _count;

    #endregion =====

    #region ===== 초기화 =====

    private void Awake()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
    }

    #endregion =====

    #region ===== 설정 =====

    /// <summary>
    /// 소지 중인 인벤토리 데이터를 슬롯에 설정한다.
    /// </summary>
    public void SetData(int itemID, int count)
    {
        _itemID = itemID;
        _count = count;
    }

    /// <summary>
    /// 슬롯의 인벤토리 데이터를 초기화한다.
    /// </summary>
    public void ClearData()
    {
        _itemID = 0;
        _count = 0;
    }

    #endregion =====

    #region ===== UI 갱신 =====

    /// <summary>
    /// 인벤토리 정보를 기반으로 슬롯 UI를 갱신한다.
    /// </summary>
    public override void UpdateUI()
    {
        if (_itemID <= 0)
        {
            HideItem();
            return;
        }

        ShowItem();
    }

    /// <summary>
    /// 아이템이 없는 슬롯의 UI를 숨긴다.
    /// </summary>
    private void HideItem()
    {
        Get<Image>(Images.ItemImage).gameObject.SetActive(false);
        Get<Image>(Images.BadgeImage).gameObject.SetActive(false);
        Get<TMP_Text>(Texts.ItemCountText).gameObject.SetActive(false);
    }

    /// <summary>
    /// 현재 아이템 정보를 슬롯 UI에 표시한다.
    /// </summary>
    private void ShowItem()
    {
        string resourceName =
            Managers.Table.Item.GetItemResourceName(_itemID);

        Get<Image>(Images.ItemImage).sprite =
            Managers.Resource.Load<Sprite>(
                ResourceKey.Path.ItemSprite + resourceName);

        Get<Image>(Images.ItemImage).gameObject.SetActive(true);

        Get<TMP_Text>(Texts.ItemCountText).text =
            _count.ToString("N0");

        Get<TMP_Text>(Texts.ItemCountText).gameObject.SetActive(_count > 0);
    }

    #endregion ===== UI 갱신 =====
}