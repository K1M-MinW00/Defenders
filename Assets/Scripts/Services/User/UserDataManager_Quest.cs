using System;
using System.Threading.Tasks;

public partial class UserDataManager
{
    public async Task<QuestScoreClaimResult> ClaimQuestScoreAsync(
        string questId,
        DateTimeOffset? utcNow = null)
    {
        QuestDefinition definition = DailyQuestCatalog.All.FindById(questId);
        if (definition == null || UserData?.Quest == null)
            return QuestScoreClaimResult.Fail(QuestScoreClaimFailure.InvalidQuest);

        return await RunSerializedMutationAsync(async () =>
        {
            UserQuestData next = UserDataCloner.Copy(UserData.Quest);
            QuestScoreClaimFailure failure = QuestProgressService.ClaimScore(
                next, questId, utcNow ?? DateTimeOffset.UtcNow);
            if (failure != QuestScoreClaimFailure.None)
                return QuestScoreClaimResult.Fail(failure);

            if (!await SaveSectionAsync(next, () => repository.SaveSectionsAsync(
                    CurrentUserId, new UserDataUpdate { Quest = next }), "quest score"))
                return QuestScoreClaimResult.Fail(QuestScoreClaimFailure.SaveFailed);

            UserData.Quest = next;
            RaiseQuestUpdated();
            return QuestScoreClaimResult.Success();
        });
    }

    public async Task<QuestRewardClaimResult> ClaimQuestRewardAsync(
        QuestRewardPeriod period,
        int requiredScore,
        DateTimeOffset? utcNow = null)
    {
        if (UserData?.Quest == null || GameConfig.QuestRewards == null)
            return QuestRewardClaimResult.Fail(QuestRewardClaimFailure.InvalidReward);

        return await RunSerializedMutationAsync(async () =>
        {
            QuestRewardMilestone milestone = GameConfig.QuestRewards.Get(period, requiredScore);
            if (milestone == null)
                return QuestRewardClaimResult.Fail(QuestRewardClaimFailure.InvalidReward);

            UserQuestData nextQuest = UserDataCloner.Copy(UserData.Quest);
            QuestProgressService.RefreshPeriods(nextQuest, utcNow ?? DateTimeOffset.UtcNow);
            int score = period == QuestRewardPeriod.Daily ? nextQuest.DailyScore : nextQuest.WeeklyScore;
            System.Collections.Generic.List<int> claimed = period == QuestRewardPeriod.Daily
                ? nextQuest.ClaimedDailyRewards
                : nextQuest.ClaimedWeeklyRewards;
            if (score < requiredScore)
                return QuestRewardClaimResult.Fail(QuestRewardClaimFailure.InsufficientScore);

            System.Collections.Generic.List<QuestRewardMilestone> claimableMilestones = new();
            GameConfig.QuestRewards.CollectClaimableMilestones(
                period, requiredScore, claimed, claimableMilestones);
            if (claimableMilestones.Count == 0)
                return QuestRewardClaimResult.Fail(QuestRewardClaimFailure.AlreadyClaimed);

            System.Collections.Generic.List<RewardData> rewards = new();
            foreach (QuestRewardMilestone claimable in claimableMilestones)
                rewards.AddRange(claimable.Rewards);

            RewardGrantResult grant = RewardGrantCalculator.Calculate(UserData, rewards);
            if (!grant.Succeeded)
                return QuestRewardClaimResult.Fail(QuestRewardClaimFailure.GrantFailed);
            foreach (QuestRewardMilestone claimable in claimableMilestones)
                claimed.Add(claimable.RequiredScore);

            try
            {
                await repository.SaveSectionsAsync(CurrentUserId, new UserDataUpdate
                {
                    Resources = grant.Resources,
                    Inventory = grant.Inventory,
                    Roster = grant.Roster,
                    Profile = grant.Profile,
                    Quest = nextQuest,
                });
            }
            catch
            {
                return QuestRewardClaimResult.Fail(QuestRewardClaimFailure.SaveFailed);
            }

            UserData.Resource = grant.Resources;
            UserData.Inventory = grant.Inventory;
            UserData.Roster = grant.Roster;
            UserData.Profile = grant.Profile;
            UserData.Quest = nextQuest;
            RaiseResourceUpdated();
            RaiseInventoryUpdated();
            RaiseRosterUpdated();
            RaiseProfileUpdated();
            RaiseQuestUpdated();
            return QuestRewardClaimResult.Success(rewards);
        });
    }

    public async Task<bool> RefreshQuestPeriodsAsync(DateTimeOffset? utcNow = null)
    {
        if (UserData?.Quest == null || repository == null)
            return false;

        return await RunSerializedMutationAsync(async () =>
        {
            UserQuestData next = UserDataCloner.Copy(UserData.Quest);
            if (!QuestProgressService.RefreshPeriods(next, utcNow ?? DateTimeOffset.UtcNow))
                return true;

            if (!await SaveSectionAsync(next, () => repository.SaveSectionsAsync(
                    CurrentUserId, new UserDataUpdate { Quest = next }), "quest"))
                return false;

            UserData.Quest = next;
            RaiseQuestUpdated();
            return true;
        });
    }
}
