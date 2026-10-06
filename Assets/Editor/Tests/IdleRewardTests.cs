using System;
using Firebase.Firestore;
using NUnit.Framework;
using UnityEngine;

public sealed class IdleRewardTests
{
    private IdleRewardConfigSO config;
    private readonly DateTime now = new(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc);

    [SetUp]
    public void SetUp()
    {
        config = ScriptableObject.CreateInstance<IdleRewardConfigSO>();
        config.MinimumClaimMinutes = 10;
        config.MaximumAccumulationMinutes = 720;
        config.Rates.Add(new IdleRewardRate
        {
            Type = RewardType.Gold,
            UnlockSector = 1,
            BaseHourlyAmount = 600,
            AdditionalHourlyPerSector = 400,
        });
    }

    [TearDown]
    public void TearDown() => UnityEngine.Object.DestroyImmediate(config);

    [Test]
    public void Preview_UsesCurrentSectorAndMinimumClaimTime()
    {
        UserDataRoot data = CreateData(sector: 3, elapsedMinutes: 10);

        IdleRewardPreview preview = IdleRewardUseCase.BuildPreview(data, now, config);

        Assert.That(preview.Sector, Is.EqualTo(3));
        Assert.That(preview.HourlyRates[0].BaseAmount, Is.EqualTo(1400));
        Assert.That(preview.CanClaim, Is.True);
    }

    [Test]
    public void Preview_BeforeTenMinutesCannotClaim()
    {
        IdleRewardPreview preview = IdleRewardUseCase.BuildPreview(CreateData(1, 9), now, config);
        Assert.That(preview.CanClaim, Is.False);
    }

    [Test]
    public void Preview_CapsAccumulationAtTwelveHours()
    {
        IdleRewardPreview preview = IdleRewardUseCase.BuildPreview(CreateData(1, 1000), now, config);
        Assert.That(preview.AccumulatedMinutes, Is.EqualTo(720));
        Assert.That(preview.AccumulatedRewards[0].BaseAmount, Is.EqualTo(7200));
    }

    private UserDataRoot CreateData(int sector, int elapsedMinutes) => new()
    {
        Progress = new UserProgressData { CurrentSector = sector },
        IdleReward = new UserIdleRewardData
        {
            LastClaimAt = Timestamp.FromDateTime(now.AddMinutes(-elapsedMinutes)),
        },
        Lab = new UserLabData(),
    };
}
