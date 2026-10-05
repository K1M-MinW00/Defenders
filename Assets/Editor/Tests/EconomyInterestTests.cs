using NUnit.Framework;
using UnityEngine;

public sealed class EconomyInterestTests
{
    private EconomyConfig config;

    [SetUp]
    public void SetUp()
    {
        config = ScriptableObject.CreateInstance<EconomyConfig>();
        config.bonusPer10 = 1;
        config.bonusCap = 5;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(config);
    }

    [TestCase(0, 0)]
    [TestCase(9, 0)]
    [TestCase(10, 1)]
    [TestCase(49, 4)]
    [TestCase(50, 5)]
    [TestCase(999, 5)]
    public void CalculateInterestBonus_UsesTenGoldStepsAndCap(int gold, int expected)
    {
        Assert.AreEqual(expected, config.CalculateInterestBonus(gold));
    }

    [Test]
    public void WaveRewardBreakdown_UsesGoldBeforeWaveReward()
    {
        config.initialGold = 25;
        config.normalReward = 14;
        GameObject owner = new("EconomyManagerTest");

        try
        {
            EconomyManager economy = owner.AddComponent<EconomyManager>();
            Assert.IsTrue(economy.Init(config));

            Assert.IsTrue(economy.TryGetWaveRewardBreakdown(
                WaveType.Normal,
                out int waveReward,
                out int bonusReward));

            Assert.AreEqual(14, waveReward);
            Assert.AreEqual(2, bonusReward);
            Assert.AreEqual(25, economy.CurrentGold);
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }
}
