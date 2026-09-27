using System;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

public sealed class GemShopPurchaseTests
{
    private const string UserId = "shop-test-user";
    private ShopProductData product;

    [SetUp]
    public void SetUp()
    {
        product = ScriptableObject.CreateInstance<ShopProductData>();
        product.ProductId = "exchange_gold_small";
        product.DisplayName = "골드 상자";
        product.IsEnabled = true;
        product.PurchaseType = ShopPurchaseType.Gem;
        product.CostAmount = 10;
        product.ResetPeriod = ShopResetPeriod.Daily;
        product.PurchaseLimit = 2;
        product.Rewards = new()
        {
            new RewardData { Type = RewardType.Gold, Amount = 500 },
        };
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(product);
    }

    [Test]
    public void DailyPeriod_ChangesAtKstNine()
    {
        DateTimeOffset beforeReset = new(2026, 9, 23, 8, 59, 0, TimeSpan.FromHours(9));
        DateTimeOffset afterReset = new(2026, 9, 23, 9, 0, 0, TimeSpan.FromHours(9));

        Assert.That(
            ShopTimePolicy.GetPeriodKey(ShopResetPeriod.Daily, beforeReset.ToUniversalTime()),
            Is.EqualTo("D:20260922"));
        Assert.That(
            ShopTimePolicy.GetPeriodKey(ShopResetPeriod.Daily, afterReset.ToUniversalTime()),
            Is.EqualTo("D:20260923"));
    }

    [Test]
    public async Task Purchase_AtomicallyChargesGemGrantsRewardAndRecordsStock()
    {
        UserDataRoot data = CreateUserData(gem: 20, gold: 100);
        RecordingRepository repository = new();
        GemShopPurchaseUseCase useCase = new(repository, UserId, data);

        ShopPurchaseResult result = await useCase.ExecuteAsync(
            product,
            new DateTimeOffset(2026, 9, 23, 0, 0, 0, TimeSpan.Zero));

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.RemainingPurchases, Is.EqualTo(1));
        Assert.That(data.Resource.Gem, Is.EqualTo(10));
        Assert.That(data.Resource.Gold, Is.EqualTo(600));
        Assert.That(data.Shop.Purchases, Has.Count.EqualTo(1));
        Assert.That(repository.SaveSectionsCallCount, Is.EqualTo(1));
        Assert.That(repository.LastUpdate.Shop, Is.Not.Null);
    }

    [Test]
    public async Task Purchase_RejectsInsufficientGemWithoutSaving()
    {
        UserDataRoot data = CreateUserData(gem: 9, gold: 100);
        RecordingRepository repository = new();
        GemShopPurchaseUseCase useCase = new(repository, UserId, data);

        ShopPurchaseResult result = await useCase.ExecuteAsync(product, DateTimeOffset.UtcNow);

        Assert.That(result.Failure, Is.EqualTo(ShopPurchaseFailure.InsufficientGem));
        Assert.That(data.Resource.Gem, Is.EqualTo(9));
        Assert.That(repository.SaveSectionsCallCount, Is.Zero);
    }

    [Test]
    public async Task Purchase_SaveFailureDoesNotMutateInMemoryData()
    {
        UserDataRoot data = CreateUserData(gem: 20, gold: 100);
        RecordingRepository repository = new() { ThrowOnSave = true };
        GemShopPurchaseUseCase useCase = new(repository, UserId, data);

        ShopPurchaseResult result = await useCase.ExecuteAsync(product, DateTimeOffset.UtcNow);

        Assert.That(result.Failure, Is.EqualTo(ShopPurchaseFailure.SaveFailed));
        Assert.That(data.Resource.Gem, Is.EqualTo(20));
        Assert.That(data.Resource.Gold, Is.EqualTo(100));
        Assert.That(data.Shop.Purchases, Is.Empty);
    }

    [Test]
    public async Task Purchase_RejectsThirdPurchaseInSamePeriod()
    {
        UserDataRoot data = CreateUserData(gem: 100, gold: 0);
        RecordingRepository repository = new();
        GemShopPurchaseUseCase useCase = new(repository, UserId, data);
        DateTimeOffset now = new(2026, 9, 23, 0, 0, 0, TimeSpan.Zero);

        Assert.That((await useCase.ExecuteAsync(product, now)).Succeeded, Is.True);
        Assert.That((await useCase.ExecuteAsync(product, now)).Succeeded, Is.True);
        ShopPurchaseResult third = await useCase.ExecuteAsync(product, now);

        Assert.That(third.Failure, Is.EqualTo(ShopPurchaseFailure.SoldOut));
        Assert.That(data.Resource.Gem, Is.EqualTo(80));
        Assert.That(data.Resource.Gold, Is.EqualTo(1000));
        Assert.That(repository.SaveSectionsCallCount, Is.EqualTo(2));
    }

    private static UserDataRoot CreateUserData(int gem, int gold)
    {
        return new UserDataRoot
        {
            SchemaVersion = UserDataSchema.CurrentVersion,
            Profile = new UserProfileData(),
            Resource = new UserResourceData { Gem = gem, Gold = gold, MaxFuel = 100 },
            Roster = new UserRosterData(),
            Progress = new UserProgressData(),
            Inventory = new UserInventoryData(),
            Gacha = new UserGachaData(),
            Ad = new UserAdData(),
            Shop = new UserShopData(),
        };
    }

    private sealed class RecordingRepository : IUserDataRepository
    {
        public bool ThrowOnSave { get; set; }
        public int SaveSectionsCallCount { get; private set; }
        public UserDataUpdate LastUpdate { get; private set; }

        public Task<UserDataLoadResult> LoadAsync(string userId) =>
            Task.FromResult(UserDataLoadResult.NotFound());
        public Task CreateAsync(string userId, UserDataRoot data) => Task.CompletedTask;
        public Task SaveAllAsync(string userId, UserDataRoot data) => Task.CompletedTask;
        public Task SaveProfileAsync(string userId, UserProfileData profile) => Task.CompletedTask;
        public Task SaveResourcesAsync(string userId, UserResourceData resources) => Task.CompletedTask;
        public Task SaveProgressAsync(string userId, UserProgressData progress) => Task.CompletedTask;
        public Task SaveRosterAsync(string userId, UserRosterData roster) => Task.CompletedTask;

        public Task SaveSectionsAsync(string userId, UserDataUpdate update)
        {
            SaveSectionsCallCount++;
            LastUpdate = update;
            if (ThrowOnSave)
                throw new InvalidOperationException("Simulated save failure.");
            return Task.CompletedTask;
        }
    }
}
