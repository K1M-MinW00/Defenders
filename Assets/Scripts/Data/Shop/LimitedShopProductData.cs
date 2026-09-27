using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Products/Limited Product")]
public sealed class LimitedShopProductData : ShopProductData
{
    [Header("Limited presentation")]
    public Sprite Portrait;
    [TextArea(2, 5)] public string Introduction;
}
