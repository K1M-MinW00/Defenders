using NUnit.Framework;

public sealed class DamageResolverTests
{
    [Test]
    public void Resolve_ClampsAppliedDamageToRemainingHealth()
    {
        DamageResolution result = DamageResolver.Resolve(
            new DamageRequest(15f),
            currentHp: 10f,
            targetIsDead: false);

        Assert.That(result.CanApply, Is.True);
        Assert.That(result.ModifiedAmount, Is.EqualTo(15f));
        Assert.That(result.AppliedAmount, Is.EqualTo(10f));
        Assert.That(result.IsLethal, Is.True);
    }

    [Test]
    public void Resolve_AppliesDefensiveModifierBeforeHealthClamp()
    {
        DamageResolution result = DamageResolver.Resolve(
            new DamageRequest(10f),
            currentHp: 20f,
            targetIsDead: false,
            modifier: amount => amount * 0.5f);

        Assert.That(result.CanApply, Is.True);
        Assert.That(result.ModifiedAmount, Is.EqualTo(5f));
        Assert.That(result.AppliedAmount, Is.EqualTo(5f));
        Assert.That(result.IsLethal, Is.False);
    }

    [Test]
    public void Resolve_ReportsFullyPreventedDamage()
    {
        DamageResolution result = DamageResolver.Resolve(
            new DamageRequest(10f),
            currentHp: 20f,
            targetIsDead: false,
            modifier: _ => 0f);

        Assert.That(result.CanApply, Is.False);
        Assert.That(result.RejectReason, Is.EqualTo(DamageRejectReason.FullyPrevented));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    public void Resolve_RejectsInvalidRequestedAmount(float amount)
    {
        DamageResolution result = DamageResolver.Resolve(
            new DamageRequest(amount),
            currentHp: 10f,
            targetIsDead: false);

        Assert.That(result.CanApply, Is.False);
        Assert.That(result.RejectReason, Is.EqualTo(DamageRejectReason.InvalidAmount));
    }

    [Test]
    public void Resolve_RejectsAlreadyDeadTarget()
    {
        DamageResolution result = DamageResolver.Resolve(
            new DamageRequest(10f),
            currentHp: 0f,
            targetIsDead: true);

        Assert.That(result.CanApply, Is.False);
        Assert.That(result.RejectReason, Is.EqualTo(DamageRejectReason.TargetAlreadyDead));
    }
}
