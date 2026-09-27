using NUnit.Framework;

public sealed class CombatTargetContractTests
{
    [Test]
    public void Controllers_ExposeCommonCombatTargetContract()
    {
        Assert.That(typeof(ICombatTarget).IsAssignableFrom(typeof(UnitController)), Is.True);
        Assert.That(typeof(ICombatTarget).IsAssignableFrom(typeof(MonsterController)), Is.True);
    }

    [Test]
    public void HealthComponents_ExposeCommonHealthContract()
    {
        Assert.That(typeof(ICombatHealth).IsAssignableFrom(typeof(UnitHealth)), Is.True);
        Assert.That(typeof(ICombatHealth).IsAssignableFrom(typeof(MonsterHealth)), Is.True);
        Assert.That(typeof(IDamageable).IsAssignableFrom(typeof(ICombatHealth)), Is.True);
    }
}
