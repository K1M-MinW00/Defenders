using UnityEngine;
using UnityEngine.UI;

public sealed class ExchangeProductView : ShopProductView
{
    [Header("Exchange UI")]
    [SerializeField] private Image productImage;
    [SerializeField] private Transform rewardContainer;
    [SerializeField] private ShopRewardSlotView rewardSlotPrefab;

    protected override void BindProduct(ShopProductData product)
    {
        ExchangeShopProductData exchange = product as ExchangeShopProductData;
        if (exchange == null)
        {
            Debug.LogError($"[{nameof(ExchangeProductView)}] ExchangeShopProductData가 필요합니다.", this);
            return;
        }

        Sprite image = exchange.ProductImage != null
            ? exchange.ProductImage
            : ResolvePrimaryRewardIcon(exchange);
        ApplySprite(productImage, image);
        RebuildRewardSlots(exchange, rewardContainer, rewardSlotPrefab);
    }
}
