using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Shop Group")]
public class ShopProductGroup : ScriptableObject
{
    public List<ShopProductData> LimitedProducts;

    public List<ShopProductData> DailyPackages;

    public List<ShopProductData> WeeklyPackages;

    public List<ShopProductData> MonthlyPackages;

    public List<ShopProductData> RechargeProducts;

    public List<ShopProductData> ExchangeProducts;
}
