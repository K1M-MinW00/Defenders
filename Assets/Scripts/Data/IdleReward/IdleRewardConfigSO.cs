using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class IdleRewardRate
{
    public RewardType Type;
    public string ItemId;
    [Min(1)] public int UnlockSector = 1;
    [Min(0)] public int BaseHourlyAmount;
    [Min(0)] public int AdditionalHourlyPerSector;

    public int GetHourlyAmount(int sector) =>
        Mathf.Max(0, BaseHourlyAmount + Mathf.Max(0, sector - UnlockSector) * AdditionalHourlyPerSector);
}

[CreateAssetMenu(fileName = "IdleRewardConfig", menuName = "Game/Idle Reward/Config")]
public sealed class IdleRewardConfigSO : ScriptableObject
{
    [Min(1)] public int MinimumClaimMinutes = 10;
    [Min(1)] public int MaximumAccumulationMinutes = 720;
    public List<IdleRewardRate> Rates = new();

    public IEnumerable<IdleRewardRate> GetUnlockedRates(int sector) =>
        Rates.Where(rate => rate != null && sector >= rate.UnlockSector);

    public bool TryValidate(IItemCatalog items, out string error)
    {
        if (MinimumClaimMinutes < 1 || MaximumAccumulationMinutes < MinimumClaimMinutes || Rates == null || Rates.Count == 0)
        {
            error = "Idle reward timing or rates are invalid.";
            return false;
        }

        foreach (IdleRewardRate rate in Rates)
        {
            if (rate == null || rate.UnlockSector < 1 || rate.BaseHourlyAmount < 0 || rate.AdditionalHourlyPerSector < 0)
            {
                error = "An idle reward rate contains invalid values.";
                return false;
            }
            if (rate.Type == RewardType.Item && items?.Get(rate.ItemId) == null)
            {
                error = $"Idle reward item was not found: {rate.ItemId}";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }
}
