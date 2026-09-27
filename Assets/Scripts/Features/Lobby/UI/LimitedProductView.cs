using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LimitedProductView : ShopProductView
{
    [Header("Limited UI")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Transform rewardContainer;
    [SerializeField] private ShopRewardSlotView rewardSlotPrefab;

    protected override void BindProduct(ShopProductData product)
    {
        LimitedShopProductData limited = product as LimitedShopProductData;
        if (limited == null)
        {
            Debug.LogError($"[{nameof(LimitedProductView)}] LimitedShopProductData가 필요합니다.", this);
            return;
        }

        ApplySprite(portraitImage, limited.Portrait);
        if (descriptionText != null)
            descriptionText.text = !string.IsNullOrWhiteSpace(limited.Introduction)
                ? limited.Introduction
                : limited.Description;

        RebuildRewardSlots(limited, rewardContainer, rewardSlotPrefab);
    }
}
