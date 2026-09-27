using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Shop Product")]
public class ShopProductData : ScriptableObject
{
    [Header("Identity")]
    public string ProductId;
    public string DisplayName;
    [TextArea] public string Description;
    public ShopTabType Tab;
    public int SortOrder;
    public bool IsEnabled = true;

    [Header("Purchase")]
    public ShopPurchaseType PurchaseType;
    public string IAPProductId;
    public int Price;
    public int CostAmount;

    [Header("Availability")]
    public ShopResetPeriod ResetPeriod;
    [Min(0)] public int PurchaseLimit;
    [Tooltip("ISO-8601 UTC. Empty means no start restriction.")]
    public string SaleStartUtc;
    [Tooltip("ISO-8601 UTC. Empty means no end restriction.")]
    public string SaleEndUtc;

    [Header("Rewards")]
    public List<RewardData> Rewards;

    public bool IsGemProduct => PurchaseType == ShopPurchaseType.Gem;

    private void OnValidate()
    {
        CostAmount = Mathf.Max(0, CostAmount);
        PurchaseLimit = Mathf.Max(0, PurchaseLimit);
        SortOrder = Mathf.Max(0, SortOrder);
    }
}
