using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Shop Product")]
public class ShopProductData : ScriptableObject
{
    [Header("기본")]
    public string ProductId;
    public string DisplayName;
    public Sprite Icon;

    [Header("구매 방식")]
    public ShopPurchaseType PurchaseType;

    [Header("인앱결제")]
    public string IAPProductId;
    public int Price;

    [Header("광고")]
    public int DailyLimit;

    [Header("재화 구매")]
    public RewardType CostType;
    public int CostAmount;

    [Header("보상")]
    public List<ShopRewardData> Rewards;
}