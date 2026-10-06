using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class IdleRewardEntry
{
    public RewardType Type { get; set; }
    public string ItemId { get; set; }
    public int BaseAmount { get; set; }
    public int BonusAmount { get; set; }
    public int TotalAmount => BaseAmount + BonusAmount;
}

public sealed class IdleRewardPreview
{
    public int Sector { get; set; }
    public int AccumulatedMinutes { get; set; }
    public int MaxMinutes { get; set; }
    public IReadOnlyList<IdleRewardEntry> HourlyRates { get; set; }
    public IReadOnlyList<IdleRewardEntry> AccumulatedRewards { get; set; }
    public bool CanClaim => AccumulatedMinutes >= IdleRewardUseCase.MinimumClaimMinutes && AccumulatedRewards.Any(x => x.TotalAmount > 0);
}

public enum IdleRewardClaimFailure { None, NotReady, InvalidData, SaveFailed }

public sealed class IdleRewardUseCase
{
    public const int MaximumAccumulationMinutes = 12 * 60;
    public const int MinimumClaimMinutes = 1;

    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;

    public IdleRewardUseCase(IUserDataRepository repository, string userId, UserDataRoot userData)
    {
        this.repository = repository;
        this.userId = userId;
        this.userData = userData;
    }

    public IdleRewardPreview BuildPreview(DateTime utcNow)
    {
        if (userData?.Progress == null || userData.IdleReward == null)
            return null;

        DateTime lastClaim = userData.IdleReward.LastClaimAt.ToDateTime();
        int minutes = Math.Clamp((int)(utcNow - lastClaim).TotalMinutes, 0, MaximumAccumulationMinutes);
        int sector = Math.Max(1, userData.Progress.CurrentSector);
        float bonusRate = Math.Max(0f, LabBonusProvider.GetTotal(LabEffectType.IdleRewardPercent)) / 100f;
        List<(RewardType type, string id, int hourly)> rates = BuildHourlyRates(sector);
        return new IdleRewardPreview
        {
            Sector = sector,
            AccumulatedMinutes = minutes,
            MaxMinutes = MaximumAccumulationMinutes,
            HourlyRates = rates.Select(x => BuildEntry(x.type, x.id, x.hourly, bonusRate)).ToList(),
            AccumulatedRewards = rates.Select(x => BuildEntry(x.type, x.id, x.hourly * minutes / 60, bonusRate)).Where(x => x.TotalAmount > 0).ToList(),
        };
    }

    public async Task<IdleRewardClaimFailure> ClaimAsync(DateTime utcNow)
    {
        IdleRewardPreview preview = BuildPreview(utcNow);
        if (preview == null) return IdleRewardClaimFailure.InvalidData;
        if (!preview.CanClaim) return IdleRewardClaimFailure.NotReady;

        List<RewardData> rewards = preview.AccumulatedRewards.Select(entry => new RewardData
        {
            Type = entry.Type,
            Id = entry.ItemId,
            Amount = entry.TotalAmount,
        }).ToList();
        RewardGrantResult grant = RewardGrantCalculator.Calculate(userData, rewards);
        if (!grant.Succeeded) return IdleRewardClaimFailure.InvalidData;

        UserIdleRewardData nextIdle = new() { LastClaimAt = Timestamp.FromDateTime(utcNow) };
        try
        {
            await repository.SaveSectionsAsync(userId, new UserDataUpdate
            {
                Resources = grant.Resources,
                Inventory = grant.Inventory,
                IdleReward = nextIdle,
            });
        }
        catch
        {
            return IdleRewardClaimFailure.SaveFailed;
        }

        userData.Resource = grant.Resources;
        userData.Inventory = grant.Inventory;
        userData.IdleReward = nextIdle;
        return IdleRewardClaimFailure.None;
    }

    private static IdleRewardEntry BuildEntry(RewardType type, string id, int baseAmount, float bonusRate) => new()
    {
        Type = type,
        ItemId = id,
        BaseAmount = Math.Max(0, baseAmount),
        BonusAmount = Math.Max(0, (int)Math.Floor(baseAmount * bonusRate)),
    };

    private static List<(RewardType, string, int)> BuildHourlyRates(int sector)
    {
        List<(RewardType, string, int)> rates = new()
        {
            (RewardType.Gold, string.Empty, 600 + (sector - 1) * 400),
            (RewardType.ResearchMaterial, string.Empty, 20 + (sector - 1) * 5),
            (RewardType.Item, "material_exp_1", 12 + sector * 3),
            (RewardType.Item, "material_prom_1", 2 + sector / 2),
        };
        if (sector >= 3) rates.Add((RewardType.Item, "material_exp_2", 3 + sector));
        if (sector >= 6) rates.Add((RewardType.Item, "material_prom_2", 1 + sector / 3));
        return rates;
    }
}
