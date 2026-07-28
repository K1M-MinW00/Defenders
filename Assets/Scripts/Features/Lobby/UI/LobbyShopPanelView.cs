using System.Collections.Generic;
using UnityEngine;

public class LobbyShopPanelView : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private ShopProductGroup shopProductGroup;

    [Header("Item Prefab")]
    [SerializeField] private ShopItemView shopItemPrefab;

    [Header("Parents")]
    [SerializeField] private Transform dailyPackageParent;
    [SerializeField] private Transform weeklyPackageParent;
    [SerializeField] private Transform monthlyPackageParent;
    [SerializeField] private Transform rechargeParent;
    [SerializeField] private Transform exchangeParent;
    [SerializeField] private Transform freeRechargeParent;

    private readonly List<ShopItemView> itemViews = new();

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        ClearItems();

        CreateItems(shopProductGroup.DailyPackages, dailyPackageParent);
        CreateItems(shopProductGroup.WeeklyPackages, weeklyPackageParent);
        CreateItems(shopProductGroup.MonthlyPackages, monthlyPackageParent);
        CreateItems(shopProductGroup.RechargeProducts, rechargeParent);
        CreateItems(shopProductGroup.ExchangeProducts, exchangeParent);
        CreateItems(shopProductGroup.FreeRechargeProducts, freeRechargeParent);
    }

    private void CreateItems(List<ShopProductData> products, Transform parent)
    {
        foreach (ShopProductData product in products)
        {
            ShopItemView item = Instantiate(shopItemPrefab, parent);
            item.Initialize(product);

            itemViews.Add(item);
        }
    }

    private void ClearItems()
    {
        foreach (ShopItemView item in itemViews)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        itemViews.Clear();
    }
}