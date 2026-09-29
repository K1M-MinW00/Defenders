using NUnit.Framework;

public sealed class StackingBonusCalculatorTests
{
    [Test]
    public void Promotion_ReachesSameMaximumBonusWithFewerStacks()
    {
        const float maximumBonus = 0.3f;

        float normal = StackingBonusCalculator.Calculate(5, 5, maximumBonus);
        float promoted = StackingBonusCalculator.Calculate(4, 4, maximumBonus);

        Assert.That(normal, Is.EqualTo(maximumBonus).Within(0.0001f));
        Assert.That(promoted, Is.EqualTo(maximumBonus).Within(0.0001f));
    }

    [Test]
    public void Promotion_DistributesMaximumBonusAcrossRequiredStacks()
    {
        float bonus = StackingBonusCalculator.Calculate(1, 4, 0.3f);

        Assert.That(bonus, Is.EqualTo(0.075f).Within(0.0001f));
    }

    [TestCase(0, 4, 0.3f)]
    [TestCase(2, 0, 0.3f)]
    [TestCase(2, 4, 0f)]
    public void InvalidOrEmptyStack_ReturnsZero(int stacks, int required, float maximum)
    {
        Assert.That(StackingBonusCalculator.Calculate(stacks, required, maximum), Is.Zero);
    }
}
