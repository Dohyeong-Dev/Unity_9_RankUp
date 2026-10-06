using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 아이템 설명 패널의 위치, 내용과 크기를 관리한다. </summary>
[RequireComponent(typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(CanvasGroup))]
public class ItemDescription : MonoBehaviour
{
    #region ===== 참조 =====

    private TMP_Text _itemNameText;
    private TMP_Text _itemDescText;
    
    private LayoutElement _itemNameLayoutElement;
    private LayoutElement _itemDescLayoutElement;

    private CanvasGroup _canvasGroup;
    
    #endregion ===== 참조 =====

    [Header("위치")]
    [SerializeField]
    private Vector2 _positionOffset;

    [Header("너비")]
    [SerializeField]
    private float _preferredWidth = 300;
    
    private void Awake()
    {
        _itemNameText = gameObject.FindChild<TMP_Text>("ItemNameText");
        _itemNameLayoutElement = _itemNameText.gameObject.GetOrAddComponent<LayoutElement>();
        
        _itemDescText = gameObject.FindChild<TMP_Text>("ItemDescText");
        _itemDescLayoutElement = _itemDescText.gameObject.GetOrAddComponent<LayoutElement>();
        
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        _itemNameLayoutElement.preferredWidth = _preferredWidth;
        _itemDescLayoutElement.preferredWidth = _preferredWidth;
        
        _canvasGroup.blocksRaycasts = false;
        
        Hide();
    }

    #region ===== 표시 =====

    /// <summary> 상점의 아이템 설명을 표시한다. </summary>
    public void Show(ShopSlot shopSlot)
    {
        _itemNameText.text = Managers.Table.Item.GetItemName(shopSlot.ItemID);
        _itemDescText.text = Managers.Table.Item.GetItemDescription(shopSlot.ItemID) + "\n";
        _itemDescText.text += "\n"+ Managers.Table.Shop.GetItemSellPrice(shopSlot.ItemID);

        UpdatePosition(shopSlot.transform);

        gameObject.SetActive(true);
    }

    /// <summary> 아이템 설명을 숨긴다. </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion ===== 표시 =====

    #region ===== 위치 =====

    /// <summary> 설명창을 지정된 아이템 위치에 배치한다. </summary>
    private void UpdatePosition(Transform itemSlot)
    {
        if (itemSlot == null)
        {
            return;
        }

        transform.position = itemSlot.position + (Vector3)_positionOffset;
    }

    #endregion ===== 위치 =====
}