using NUnit.Framework;

public sealed class AccountLevelBenefitPolicyTests
{
    [TestCase(0, 0)]
    [TestCase(1, 4)]
    [TestCase(3, 12)]
    public void MaxFuelIncrease_IsFourPerGainedLevel(int gainedLevels, int expected)
    {
        Assert.That(AccountLevelBenefitPolicy.GetMaxFuelIncrease(gainedLevels), Is.EqualTo(expected));
    }

    [TestCase(5, true)]
    [TestCase(10, true)]
    [TestCase(15, true)]
    [TestCase(4, false)]
    [TestCase(6, false)]
    public void ResearchMilestone_IsEveryFiveLevels(int level, bool expected)
    {
        Assert.That(AccountLevelBenefitPolicy.IsResearchMilestone(level), Is.EqualTo(expected));
    }
}
