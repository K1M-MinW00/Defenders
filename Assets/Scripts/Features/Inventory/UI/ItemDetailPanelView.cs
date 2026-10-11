using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPanelView : MonoBehaviour, IModalDismissHandler
{
    [Header("Item")]
    [Tooltip("ItemDetail/Icon: 흰색 9-slice 프레임이며 희귀도 색상만 적용합니다.")]
    [SerializeField] private Image iconImage;
    [Tooltip("ItemDetail/Icon/Icon_Img: 실제 아이템 Sprite를 표시합니다.")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text descriptionText;

    private ModalOverlayLifecycle lifecycle;
    public bool IsVisible => Lifecycle.IsVisible;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    public void Show(ItemDataSO itemData, int count)
    {
        if (itemData == null)
            return;

        Lifecycle.Show();
        
        iconImage.color = RarityPalette.Get(itemData.Rarity);
        itemIconImage.sprite = itemData.Icon;
        itemIconImage.color = Color.white;
        itemIconImage.enabled = itemData.Icon != null;
        nameText.text = itemData.ItemName;
        descriptionText.text = itemData.Description;

        countText.text = itemData.Stackable ? $"보유 : {count}": "";
    }

    public void Hide()
    {
        Lifecycle.Close(ClearContent);
    }

    public void HideImmediately()
    {
        ClearContent();
        Lifecycle.HideImmediately();
    }

    private void ClearContent()
    {
        iconImage.color = Color.white;
        itemIconImage.sprite = null;
        itemIconImage.color = Color.white;
        itemIconImage.enabled = false;
        nameText.text = string.Empty;
        countText.text = string.Empty;
        descriptionText.text = string.Empty;

    }

    public void Dismiss() => Hide();
}
