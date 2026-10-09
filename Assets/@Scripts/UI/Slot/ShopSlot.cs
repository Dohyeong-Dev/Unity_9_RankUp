using UnityEngine;
using UnityEngine.UI;

/// <summary> 상점에서 판매 아이템의 정보를 표시하고 구매를 처리하는 슬롯 UI다. </summary>
public class ShopSlot : SlotUI
{
    public enum Images
    {
        ItemImage
    }

    public enum Buttons
    {
        BuyButton
    }

    #region ===== 상태 =====

    private int _itemID;
    public int ItemID => _itemID;

    private Item _item;
    
    #endregion =====

    #region ===== 참조 =====

    private ShopPopup _shopPopup;

    #endregion =====

    private void Awake()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
    }

    private void Start()
    {
        InitializeBuyButton();
    }

    #region ===== 초기화 =====

    private void InitializeBuyButton()
    {
        Get<Button>(Buttons.BuyButton).onClick.AddListener(OnClickBuy);
    }

    /// <summary> 판매 아이템 정보를 슬롯에 설정한다. </summary>
    public void SetData(int itemID, ShopPopup shopPopup)
    {
        _itemID = itemID;
        _item = Managers.Table.Item.GetItem(itemID);
        _shopPopup = shopPopup;
    }

    #endregion ===== 초기화 =====

    #region ===== UI 갱신 =====

    /// <summary> 아이템 정보를 기반으로 슬롯 UI를 갱신한다. </summary>
    public override void UpdateUI()
    {
        Get<Image>(Images.ItemImage).sprite = Managers.Resource.Load<Sprite>(
            ResourceKey.Path.ItemSprite + _item?.ResourceName);
    }

    #endregion ===== UI 갱신 =====

    #region ===== 구매 =====

    /// <summary> 현재 슬롯의 아이템 구매 확인창을 표시한다. </summary>
    private void OnClickBuy()
    {
        Managers.UI.OpenPopup<AlertPopup>().Set("구매하시겠습니까?", true, TryBuyItem);
    }

    /// <summary> 현재 슬롯의 아이템 구매를 요청하고 결과에 따라 처리한다. </summary>
    private void TryBuyItem()
    {
        PurchaseResult result = Managers.Data.TryPurchaseItem(_itemID);

        switch (result)
        {
            case PurchaseResult.Success:
                _shopPopup.UpdateShopUI();
                Managers.UI.OpenToastMessage("구매 완료", true);
                break;

            case PurchaseResult.InventoryFull:
                Managers.UI.OpenToastMessage("더 이상 구매하실 수 없습니다.", true);
                break;

            case PurchaseResult.NotEnoughGold:
                Managers.UI.OpenToastMessage("소지금이 부족합니다.", true);
                break;
        }
    }

    #endregion ===== 구매 =====
}