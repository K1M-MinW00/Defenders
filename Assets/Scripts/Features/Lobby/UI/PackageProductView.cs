using UnityEngine;
using UnityEngine.UI;

public sealed class PackageProductView : ShopProductView
{
    [Header("Package UI")]
    [SerializeField] private Image packageImage;
    [SerializeField] private Transform rewardContainer;
    [SerializeField] private ShopRewardSlotView rewardSlotPrefab;

    protected override void BindProduct(ShopProductData product)
    {
        PackageShopProductData package = product as PackageShopProductData;
        if (package == null)
        {
            Debug.LogError($"[{nameof(PackageProductView)}] PackageShopProductData가 필요합니다.", this);
            return;
        }

        ApplySprite(packageImage, package.PackageImage);
        RebuildRewardSlots(package, rewardContainer, rewardSlotPrefab);
    }
}
