using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Firestore;

public sealed class GemShopPurchaseUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;
    private bool isExecuting;

    public GemShopPurchaseUseCase(IUserDataRepository repository, string userId, UserDataRoot userData)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentException("User ID is null or empty.", nameof(userId))
            : userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
    }

    public ShopProductState GetState(ShopProductData product, DateTimeOffset utcNow)
    {
        if (product == null || userData.Shop == null)
            return new ShopProductState(false, false, 0, 0, null);

        string periodKey = ShopTimePolicy.GetPeriodKey(product.ResetPeriod, utcNow);
        int purchased = FindPurchase(userData.Shop, product.ProductId, periodKey)?.Count ?? 0;
        int remaining = product.PurchaseLimit <= 0
            ? -1
            : Math.Max(0, product.PurchaseLimit - purchased);

        return new ShopProductState(
            product.IsEnabled && ShopTimePolicy.IsInSalePeriod(product, utcNow),
            product.PurchaseLimit > 0 && remaining == 0,
            purchased,
            remaining,
            ShopTimePolicy.GetNextResetUtc(product.ResetPeriod, utcNow));
    }

    public async Task<ShopPurchaseResult> ExecuteAsync(ShopProductData product, DateTimeOffset utcNow)
    {
        if (isExecuting || !IsValid(product) || userData.Resource == null ||
            userData.Inventory == null || userData.Roster == null || userData.Shop == null)
            return ShopPurchaseResult.Fail(ShopPurchaseFailure.InvalidProduct);

        if (!product.IsGemProduct)
            return ShopPurchaseResult.Fail(ShopPurchaseFailure.UnsupportedPurchaseType);

        ShopProductState state = GetState(product, utcNow);
        if (!state.IsOnSale)
            return ShopPurchaseResult.Fail(ShopPurchaseFailure.NotOnSale);

        if (state.IsSoldOut)
            return ShopPurchaseResult.Fail(ShopPurchaseFailure.SoldOut);

        if (userData.Resource.Gem < product.CostAmount)
            return ShopPurchaseResult.Fail(ShopPurchaseFailure.InsufficientGem);

        RewardGrantResult grant = RewardGrantCalculator.Calculate(userData, product.Rewards);
        if (!grant.Succeeded)
        {
            return ShopPurchaseResult.Fail(
                grant.Failure == RewardGrantFailure.Overflow
                    ? ShopPurchaseFailure.Overflow
                    : ShopPurchaseFailure.InvalidReward);
        }

        try
        {
            grant.Resources.Gem = checked(grant.Resources.Gem - product.CostAmount);
        }
        catch (OverflowException)
        {
            return ShopPurchaseResult.Fail(ShopPurchaseFailure.Overflow);
        }

        UserShopData nextShop = UserDataCloner.Copy(userData.Shop);
        string periodKey = ShopTimePolicy.GetPeriodKey(product.ResetPeriod, utcNow);
        UserShopPurchaseData purchase = FindPurchase(nextShop, product.ProductId, periodKey);
        if (purchase == null)
        {
            purchase = new UserShopPurchaseData
            {
                ProductId = product.ProductId,
                PeriodKey = periodKey,
            };
            nextShop.Purchases.Add(purchase);
        }

        purchase.Count = checked(purchase.Count + 1);
        purchase.LastPurchasedAt = Timestamp.FromDateTime(utcNow.UtcDateTime);

        isExecuting = true;
        try
        {
            await repository.SaveSectionsAsync(userId, new UserDataUpdate
            {
                Resources = grant.Resources,
                Inventory = grant.Inventory,
                Roster = grant.Roster,
                Profile = grant.Profile,
                Shop = nextShop,
            });
        }
        catch
        {
            return ShopPurchaseResult.Fail(ShopPurchaseFailure.SaveFailed);
        }
        finally
        {
            isExecuting = false;
        }

        userData.Resource = grant.Resources;
        userData.Inventory = grant.Inventory;
        userData.Roster = grant.Roster;
        userData.Profile = grant.Profile;
        userData.Shop = nextShop;

        int remaining = product.PurchaseLimit <= 0
            ? -1
            : Math.Max(0, product.PurchaseLimit - purchase.Count);
        return ShopPurchaseResult.Success(remaining);
    }

    private static bool IsValid(ShopProductData product)
    {
        return product != null &&
               !string.IsNullOrWhiteSpace(product.ProductId) &&
               product.CostAmount > 0 &&
               product.Rewards != null &&
               product.Rewards.Count > 0 &&
               product.Rewards.All(reward => reward != null && reward.Amount > 0);
    }

    private static UserShopPurchaseData FindPurchase(UserShopData shop, string productId, string periodKey)
    {
        return shop?.Purchases?.FirstOrDefault(
            purchase => purchase != null &&
                        purchase.ProductId == productId &&
                        purchase.PeriodKey == periodKey);
    }
}
