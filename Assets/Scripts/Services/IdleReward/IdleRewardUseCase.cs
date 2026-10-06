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
    public int MinimumClaimMinutes { get; set; }
    public bool CanClaim => AccumulatedMinutes >= MinimumClaimMinutes && AccumulatedRewards.Any(x => x.TotalAmount > 0);
}

public enum IdleRewardClaimFailure { None, NotReady, InvalidData, SaveFailed }

public sealed class IdleRewardUseCase
{
    private readonly IIdleRewardRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;

    private readonly IdleRewardConfigSO config;

    public IdleRewardUseCase(IIdleRewardRepository repository, string userId, UserDataRoot userData, IdleRewardConfigSO config)
    {
        this.repository = repository;
        this.userId = userId;
        this.userData = userData;
        this.config = config;
    }

    public IdleRewardPreview BuildPreview(DateTime utcNow)
    {
        return BuildPreview(userData, utcNow, config);
    }

    public async Task<IdleRewardClaimFailure> ClaimAsync()
    {
        if (repository == null || config == null) return IdleRewardClaimFailure.InvalidData;
        IdleRewardTransactionResult result;
        try { result = await repository.ClaimAsync(userId, config); }
        catch { return IdleRewardClaimFailure.SaveFailed; }
        if (result.Failure != IdleRewardClaimFailure.None) return result.Failure;
        userData.Resource = result.Resources;
        userData.Inventory = result.Inventory;
        userData.IdleReward = result.IdleReward;
        return IdleRewardClaimFailure.None;
    }

    public static IdleRewardPreview BuildPreview(UserDataRoot data, DateTime utcNow, IdleRewardConfigSO config)
    {
        if (data?.Progress == null || data.IdleReward == null || config == null) return null;
        DateTime lastClaim = data.IdleReward.LastClaimAt.ToDateTime();
        int minutes = Math.Clamp((int)(utcNow - lastClaim).TotalMinutes, 0, config.MaximumAccumulationMinutes);
        int sector = Math.Max(1, data.Progress.CurrentSector);
        float bonusRate = Math.Max(0f, LabBonusProvider.GetTotal(data, LabEffectType.IdleRewardPercent)) / 100f;
        List<(RewardType type, string id, int hourly)> rates = config.GetUnlockedRates(sector)
            .Select(rate => (rate.Type, rate.ItemId, rate.GetHourlyAmount(sector))).ToList();
        return new IdleRewardPreview
        {
            Sector = sector,
            AccumulatedMinutes = minutes,
            MaxMinutes = config.MaximumAccumulationMinutes,
            MinimumClaimMinutes = config.MinimumClaimMinutes,
            HourlyRates = rates.Select(x => BuildEntry(x.type, x.id, x.hourly, bonusRate)).ToList(),
            AccumulatedRewards = rates.Select(x => BuildEntry(x.type, x.id, x.hourly * minutes / 60, bonusRate)).Where(x => x.TotalAmount > 0).ToList(),
        };
    }

    private static IdleRewardEntry BuildEntry(RewardType type, string id, int baseAmount, float bonusRate) => new()
    {
        Type = type,
        ItemId = id,
        BaseAmount = Math.Max(0, baseAmount),
        BonusAmount = Math.Max(0, (int)Math.Floor(baseAmount * bonusRate)),
    };

}
