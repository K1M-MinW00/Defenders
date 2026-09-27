using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Products/Package Product")]
public sealed class PackageShopProductData : ShopProductData
{
    [Header("Package presentation")]
    public Sprite PackageImage;
}
