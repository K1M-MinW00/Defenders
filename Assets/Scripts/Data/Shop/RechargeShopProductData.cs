using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Products/Recharge Product")]
public sealed class RechargeShopProductData : ShopProductData
{
    [Header("Recharge presentation")]
    public Sprite GemImage;
    public bool HasFirstPurchaseBonus;
    public string FirstPurchaseBonusLabel = "첫 구매 2배";
}
