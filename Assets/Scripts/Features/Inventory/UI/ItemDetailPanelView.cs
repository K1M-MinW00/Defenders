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
    private ItemDataSO currentItem;
    private int currentCount;
    public bool IsVisible => Lifecycle.IsVisible;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    private void OnEnable() => LocalizationService.LocaleChanged += HandleLocaleChanged;
    private void OnDisable() => LocalizationService.LocaleChanged -= HandleLocaleChanged;

    public void Show(ItemDataSO itemData, int count)
    {
        if (itemData == null)
            return;

        currentItem = itemData;
        currentCount = count;

        Lifecycle.Show();
        
        iconImage.color = RarityPalette.Get(itemData.Rarity);
        itemIconImage.sprite = itemData.Icon;
        itemIconImage.color = Color.white;
        itemIconImage.enabled = itemData.Icon != null;
        nameText.text = itemData.LocalizedName;
        descriptionText.text = itemData.LocalizedDescription;

        countText.text = FormatCount(itemData, count);
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
        currentItem = null;
        currentCount = 0;
        iconImage.color = Color.white;
        itemIconImage.sprite = null;
        itemIconImage.color = Color.white;
        itemIconImage.enabled = false;
        nameText.text = string.Empty;
        countText.text = string.Empty;
        descriptionText.text = string.Empty;

    }

    public void Dismiss() => Hide();

    private void HandleLocaleChanged(string _)
    {
        if (currentItem != null && IsVisible)
            Show(currentItem, currentCount);
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Items, key, arguments);

    public static string FormatCount(ItemDataSO itemData, int count) =>
        itemData != null && itemData.Stackable ? count.ToString("N0") : string.Empty;

    private void OnDestroy() => LocalizationService.LocaleChanged -= HandleLocaleChanged;
}
