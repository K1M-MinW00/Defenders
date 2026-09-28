using NUnit.Framework;

public sealed class CombatAttackContractTests
{
    [Test]
    public void UnitAndMonsterAttacks_UseCommonAttackContract()
    {
        Assert.That(typeof(ICombatAttack).IsAssignableFrom(typeof(IUnitAttack)), Is.True);
        Assert.That(typeof(ICombatAttack).IsAssignableFrom(typeof(MeleeUnitAttack)), Is.True);
        Assert.That(typeof(ICombatAttack).IsAssignableFrom(typeof(RangedUnitAttack)), Is.True);
        Assert.That(typeof(ICombatAttack).IsAssignableFrom(typeof(MeleeMonsterAttack)), Is.True);
        Assert.That(typeof(ICombatAttack).IsAssignableFrom(typeof(RangedMonsterAttack)), Is.True);
    }

    [Test]
    public void CommonAttackContract_AcceptsCommonCombatTarget()
    {
        var method = typeof(ICombatAttack).GetMethod(nameof(ICombatAttack.TryAttack));

        Assert.That(method, Is.Not.Null);
        Assert.That(method.GetParameters()[0].ParameterType, Is.EqualTo(typeof(ICombatTarget)));
        Assert.That(typeof(ICombatAttack).GetMethod(nameof(ICombatAttack.CancelAttack)), Is.Not.Null);
    }
}
