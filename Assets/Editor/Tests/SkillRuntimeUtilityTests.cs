using NUnit.Framework;

public sealed class SkillRuntimeUtilityTests
{
    [Test]
    public void UpgradeValues_ResolveExpectedStage()
    {
        var floatValue = new SkillFloatUpgradeValue(1.5f, 2.5f);
        var intValue = new SkillIntUpgradeValue(3, 5);

        Assert.That(floatValue.Resolve(false), Is.EqualTo(1.5f));
        Assert.That(floatValue.Resolve(true), Is.EqualTo(2.5f));
        Assert.That(intValue.Resolve(false), Is.EqualTo(3));
        Assert.That(intValue.Resolve(true), Is.EqualTo(5));
    }

    [Test]
    public void ProcCounter_FiresAtThresholdAndResets()
    {
        var counter = new PassiveProcCounter();

        Assert.That(counter.Register(2), Is.False);
        Assert.That(counter.Register(2), Is.True);
        Assert.That(counter.Count, Is.Zero);
    }

    [Test]
    public void Cooldown_RejectsUntilReady()
    {
        var cooldown = new PassiveCooldown();

        Assert.That(cooldown.TryConsume(10f, 2f), Is.True);
        Assert.That(cooldown.TryConsume(11f, 2f), Is.False);
        Assert.That(cooldown.TryConsume(12f, 2f), Is.True);
    }

    [Test]
    public void StackCounter_ClampsAndResets()
    {
        var stacks = new PassiveStackCounter();

        Assert.That(stacks.Add(2), Is.EqualTo(1));
        Assert.That(stacks.Add(2), Is.EqualTo(2));
        Assert.That(stacks.Add(2), Is.EqualTo(2));
        stacks.Reset();
        Assert.That(stacks.Count, Is.Zero);
    }
}
