using NUnit.Framework;

public sealed class AttackCooldownTests
{
    [Test]
    public void NewCooldown_IsReadyImmediately()
    {
        AttackCooldown cooldown = new();

        Assert.That(cooldown.IsReady(0f), Is.True);
        Assert.That(cooldown.GetRemaining(0f), Is.Zero);
    }

    [Test]
    public void Start_BlocksUntilDurationHasElapsed()
    {
        AttackCooldown cooldown = new();

        cooldown.Start(currentTime: 10f, duration: 2f);

        Assert.That(cooldown.IsReady(11.99f), Is.False);
        Assert.That(cooldown.GetRemaining(11f), Is.EqualTo(1f));
        Assert.That(cooldown.IsReady(12f), Is.True);
    }

    [Test]
    public void Reset_MakesCooldownReadyImmediately()
    {
        AttackCooldown cooldown = new();
        cooldown.Start(currentTime: 10f, duration: 5f);

        cooldown.Reset();

        Assert.That(cooldown.IsReady(10f), Is.True);
    }

    [TestCase(-1f)]
    [TestCase(float.PositiveInfinity)]
    public void Start_TreatsInvalidDurationAsZero(float duration)
    {
        AttackCooldown cooldown = new();

        cooldown.Start(currentTime: 10f, duration: duration);

        Assert.That(cooldown.IsReady(10f), Is.True);
    }
}
