using NUnit.Framework;

public class AttackAnimationSpeedTests
{
    [TestCase(0.5f, 1f)]
    [TestCase(1f, 1f)]
    [TestCase(2.5f, 2.5f)]
    [TestCase(20f, 10f)]
    public void FromAttacksPerSecond_ClampsPlaybackSpeed(float attacksPerSecond, float expected)
    {
        Assert.That(AttackAnimationSpeed.FromAttacksPerSecond(attacksPerSecond), Is.EqualTo(expected));
    }

    [Test]
    public void FromAttacksPerSecond_InvalidValueUsesDefault()
    {
        Assert.That(AttackAnimationSpeed.FromAttacksPerSecond(float.NaN), Is.EqualTo(1f));
        Assert.That(AttackAnimationSpeed.FromAttacksPerSecond(float.PositiveInfinity), Is.EqualTo(1f));
    }
}
