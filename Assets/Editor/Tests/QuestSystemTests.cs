using System;
using System.Collections.Generic;
using NUnit.Framework;

public sealed class QuestSystemTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp() => GameConfig.Initialize();

    [Test]
    public void CompletedQuest_GrantsScoreOnlyWhenClaimed_AndOnlyOnce()
    {
        UserQuestData data = new();
        DateTimeOffset now = new(2026, 10, 12, 3, 0, 0, TimeSpan.Zero);
        QuestProgressService.Apply(data, new QuestEvent(QuestEventType.UnitRecruited), now);

        Assert.That(data.DailyScore, Is.Zero);
        Assert.That(QuestProgressService.ClaimScore(data, "daily_recruit", now), Is.EqualTo(QuestScoreClaimFailure.None));
        Assert.That(data.DailyScore, Is.EqualTo(20));
        Assert.That(data.WeeklyScore, Is.EqualTo(20));
        Assert.That(QuestProgressService.ClaimScore(data, "daily_recruit", now), Is.EqualTo(QuestScoreClaimFailure.AlreadyClaimed));
        Assert.That(data.DailyScore, Is.EqualTo(20));
    }

    [Test]
    public void LoginQuest_CompletesOncePerDailyPeriod_WithoutAutomaticallyGrantingScore()
    {
        UserQuestData data = new();
        DateTimeOffset now = new(2026, 10, 12, 3, 0, 0, TimeSpan.Zero);

        Assert.That(QuestProgressService.Apply(data, new QuestEvent(QuestEventType.Login), now), Is.True);
        Assert.That(QuestProgressService.GetProgress(data, "daily_login"), Is.EqualTo(1));
        Assert.That(data.DailyScore, Is.Zero);
        Assert.That(QuestProgressService.Apply(data, new QuestEvent(QuestEventType.Login), now), Is.False);
    }

    [Test]
    public void FuelProgress_CompletesBothSharedCounterQuests_WithoutAutomaticallyGrantingScore()
    {
        UserQuestData data = new();
        DateTimeOffset now = new(2026, 10, 12, 3, 0, 0, TimeSpan.Zero);

        Assert.That(QuestProgressService.Apply(data, new QuestEvent(QuestEventType.FuelConsumed, 50), now), Is.True);
        Assert.That(data.DailyScore, Is.Zero);
        Assert.That(data.WeeklyScore, Is.Zero);
        Assert.That(QuestProgressService.GetProgress(data, "daily_fuel_50"), Is.EqualTo(50));

        QuestProgressService.Apply(data, new QuestEvent(QuestEventType.FuelConsumed, 100), now);
        Assert.That(QuestProgressService.GetProgress(data, "daily_fuel_150"), Is.EqualTo(150));
        Assert.That(data.DailyScore, Is.Zero);

        QuestProgressService.Apply(data, new QuestEvent(QuestEventType.FuelConsumed, 100), now);
        Assert.That(data.DailyScore, Is.Zero);
    }

    [Test]
    public void DailyReset_PreservesWeeklyScore()
    {
        UserQuestData data = new();
        DateTimeOffset monday = new(2026, 10, 12, 3, 0, 0, TimeSpan.Zero);
        QuestProgressService.Apply(data, new QuestEvent(QuestEventType.UnitRecruited), monday);
        data.DailyScore = 20;
        data.WeeklyScore = 20;

        QuestProgressService.RefreshPeriods(data, monday.AddDays(1));

        Assert.That(data.DailyScore, Is.Zero);
        Assert.That(data.WeeklyScore, Is.EqualTo(20));
        Assert.That(data.Progress, Is.Empty);
    }

    [Test]
    public void SundayReset_ResetsDailyAndWeeklyAtKoreaMidnight()
    {
        UserQuestData data = new();
        DateTimeOffset saturdayKst = new(2026, 10, 17, 23, 59, 0, TimeSpan.FromHours(9));
        QuestProgressService.Apply(data, new QuestEvent(QuestEventType.UnitRecruited), saturdayKst.ToUniversalTime());

        DateTimeOffset sundayKst = saturdayKst.AddMinutes(1);
        QuestProgressService.RefreshPeriods(data, sundayKst.ToUniversalTime());

        Assert.That(data.DailyScore, Is.Zero);
        Assert.That(data.WeeklyScore, Is.Zero);
        Assert.That(data.DailyPeriodKey, Is.EqualTo("D:20261018"));
        Assert.That(data.WeeklyPeriodKey, Is.EqualTo("W:20261018"));
    }

    [Test]
    public void FuelRefund_RevertsCompletedScoresCrossedByRefund()
    {
        UserQuestData data = new();
        DateTimeOffset now = new(2026, 10, 12, 3, 0, 0, TimeSpan.Zero);
        QuestProgressService.Apply(data, new QuestEvent(QuestEventType.FuelConsumed, 150), now);
        foreach (UserQuestProgressData progress in data.Progress)
            progress.ScoreGranted = true;
        data.DailyScore = 40;
        data.WeeklyScore = 40;
        Assert.That(data.DailyScore, Is.EqualTo(40));

        QuestProgressService.Revert(data, new QuestEvent(QuestEventType.FuelConsumed, 110), now);

        Assert.That(data.DailyScore, Is.Zero);
        Assert.That(data.WeeklyScore, Is.Zero);
        Assert.That(QuestProgressService.GetProgress(data, "daily_fuel_150"), Is.EqualTo(40));
        Assert.That(QuestProgressService.GetProgress(data, "daily_fuel_50"), Is.EqualTo(0));
    }

    [Test]
    public void HigherReward_CollectsEveryUnclaimedLowerMilestone()
    {
        List<QuestRewardMilestone> claimable = new();

        GameConfig.QuestRewards.CollectClaimableMilestones(
            QuestRewardPeriod.Daily,
            80,
            new[] { 20 },
            claimable);

        Assert.That(claimable.ConvertAll(reward => reward.RequiredScore),
            Is.EqualTo(new[] { 40, 60, 80 }));
    }
}
