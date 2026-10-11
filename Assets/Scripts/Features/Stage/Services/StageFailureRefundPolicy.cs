using System.Collections.Generic;

public static class StageFailureRefundPolicy
{
    public const int FuelRefundPercent = 90;

    public static int CalculateFuelRefund(int entryFuelCost)
    {
        long safeCost = System.Math.Max(0, entryFuelCost);
        return (int)(safeCost * FuelRefundPercent / 100L);
    }

    public static IReadOnlyList<RewardData> BuildRewards(
        IReadOnlyList<RewardData> configuredRewards,
        int entryFuelCost)
    {
        List<RewardData> rewards = new();
        if (configuredRewards != null)
        {
            foreach (RewardData reward in configuredRewards)
            {
                if (reward == null || reward.Type == RewardType.Fuel)
                    continue;
                rewards.Add(reward);
            }
        }

        int refund = CalculateFuelRefund(entryFuelCost);
        if (refund > 0)
            rewards.Add(new RewardData { Type = RewardType.Fuel, Amount = refund });

        return rewards;
    }
}
