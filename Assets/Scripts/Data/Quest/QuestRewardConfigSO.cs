using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum QuestRewardPeriod { Daily, Weekly }

[Serializable]
public sealed class QuestRewardMilestone
{
    [Min(1)] public int RequiredScore;
    public List<RewardData> Rewards = new();
}

[CreateAssetMenu(fileName = "QuestRewardConfig", menuName = "Game/Quest/Reward Config")]
public sealed class QuestRewardConfigSO : ScriptableObject
{
    [Header("Reward Icons")]
    public Sprite DailyClosedIcon;
    public Sprite DailyOpenIcon;
    public Sprite WeeklyClosedIcon;
    public Sprite WeeklyOpenIcon;

    public List<QuestRewardMilestone> DailyRewards = new();
    public List<QuestRewardMilestone> WeeklyRewards = new();

    public QuestRewardMilestone Get(QuestRewardPeriod period, int requiredScore) =>
        GetRewards(period).FirstOrDefault(entry => entry.RequiredScore == requiredScore);

    public IReadOnlyList<QuestRewardMilestone> GetRewards(QuestRewardPeriod period) =>
        period == QuestRewardPeriod.Daily ? DailyRewards : WeeklyRewards;

    public void CollectClaimableMilestones(QuestRewardPeriod period, int upToScore,
        IReadOnlyCollection<int> claimedScores, List<QuestRewardMilestone> destination)
    {
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        destination.Clear();
        IReadOnlyList<QuestRewardMilestone> milestones = GetRewards(period);
        for (int i = 0; i < milestones.Count; i++)
        {
            QuestRewardMilestone milestone = milestones[i];
            if (milestone == null || milestone.RequiredScore > upToScore ||
                ContainsScore(claimedScores, milestone.RequiredScore))
                continue;
            destination.Add(milestone);
        }
    }

    public Sprite GetIcon(QuestRewardPeriod period, bool claimed) => period switch
    {
        QuestRewardPeriod.Weekly when claimed => WeeklyOpenIcon,
        QuestRewardPeriod.Weekly => WeeklyClosedIcon,
        _ when claimed => DailyOpenIcon,
        _ => DailyClosedIcon,
    };

    private static bool ContainsScore(IReadOnlyCollection<int> scores, int score)
    {
        if (scores == null) return false;
        foreach (int value in scores)
            if (value == score)
                return true;
        return false;
    }

    public bool TryValidate(IItemCatalog items, out string error)
    {
        if (DailyClosedIcon == null || DailyOpenIcon == null || WeeklyClosedIcon == null || WeeklyOpenIcon == null)
        {
            error = "Quest reward icons are missing.";
            return false;
        }
        if (!ValidateList(DailyRewards, items, out error) || !ValidateList(WeeklyRewards, items, out error))
            return false;
        error = string.Empty;
        return true;
    }

    private static bool ValidateList(List<QuestRewardMilestone> milestones, IItemCatalog items, out string error)
    {
        if (milestones == null || milestones.Count != 5 || milestones.Select(m => m?.RequiredScore ?? 0).Distinct().Count() != milestones.Count)
        {
            error = "Quest reward milestones must contain five unique entries.";
            return false;
        }
        foreach (QuestRewardMilestone milestone in milestones)
        {
            if (milestone.RequiredScore <= 0 || milestone.Rewards == null || milestone.Rewards.Count == 0)
            {
                error = "A quest reward milestone is empty or invalid.";
                return false;
            }
            foreach (RewardData reward in milestone.Rewards)
            {
                if (reward == null || reward.Amount <= 0)
                {
                    error = "A quest reward is invalid.";
                    return false;
                }
                if (reward.Type == RewardType.Item && items?.Get(reward.Id) == null)
                {
                    error = $"Quest reward item was not found: {reward.Id}";
                    return false;
                }
            }
        }
        error = string.Empty;
        return true;
    }
}
