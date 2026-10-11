using System.Collections.Generic;
using System.Linq;
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

    [Header("Utility / Context Products")]
    public List<ShopProductData> UtilityProducts;

    public ShopProductData GetById(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            return null;

        return EnumerateAll().FirstOrDefault(product =>
            product != null && product.ProductId == productId);
    }

    private IEnumerable<ShopProductData> EnumerateAll()
    {
        IEnumerable<ShopProductData>[] groups =
        {
            LimitedProducts,
            DailyPackages,
            WeeklyPackages,
            MonthlyPackages,
            RechargeProducts,
            ExchangeProducts,
            UtilityProducts,
        };

        return groups.Where(group => group != null).SelectMany(group => group);
    }
}
