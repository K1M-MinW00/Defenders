using NUnit.Framework;

public sealed class MonsterStatsTests
{
    [Test]
    public void RuntimeCopy_DoesNotMutateSourceStats()
    {
        MonsterStats source = new()
        {
            maxHp = 100f,
            moveSpeed = 2f,
            atkDamage = 10f,
            atkRange = 3f,
            atkPerSec = 1.5f,
        };

        MonsterStats runtime = source.CreateRuntimeCopy();
        runtime.maxHp = 25f;
        runtime.atkDamage = 50f;

        Assert.That(runtime, Is.Not.SameAs(source));
        Assert.That(source.maxHp, Is.EqualTo(100f));
        Assert.That(source.atkDamage, Is.EqualTo(10f));
        Assert.That(runtime.moveSpeed, Is.EqualTo(source.moveSpeed));
        Assert.That(runtime.atkRange, Is.EqualTo(source.atkRange));
        Assert.That(runtime.atkPerSec, Is.EqualTo(source.atkPerSec));
    }

    [Test]
    public void Calculator_ClampsUnsafeRuntimeValues()
    {
        MonsterStats source = new()
        {
            maxHp = -10f,
            moveSpeed = -2f,
            atkDamage = -3f,
            atkRange = -4f,
            atkPerSec = 0f,
        };

        MonsterStats result = MonsterStatCalculator.Calculate(source);

        Assert.That(result.maxHp, Is.GreaterThan(0f));
        Assert.That(result.moveSpeed, Is.Zero);
        Assert.That(result.atkDamage, Is.Zero);
        Assert.That(result.atkRange, Is.Zero);
        Assert.That(result.atkPerSec, Is.GreaterThan(0f));
        Assert.That(source.maxHp, Is.EqualTo(-10f));
    }
}
