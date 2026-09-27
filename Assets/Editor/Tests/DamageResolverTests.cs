using NUnit.Framework;
using UnityEngine;

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

    [TestCase(DamageOrigin.BasicAttack)]
    [TestCase(DamageOrigin.Skill)]
    public void Resolve_AppliesCriticalDamageToEligibleOrigins(DamageOrigin origin)
    {
        FakeDamageSource source = new(criticalChance: 0.1f, criticalDamageMultiplier: 1.25f);

        DamageResolution result = DamageResolver.Resolve(
            new DamageRequest(10f, source, origin),
            currentHp: 20f,
            targetIsDead: false,
            randomValueProvider: () => 0.05f);

        Assert.That(result.IsCritical, Is.True);
        Assert.That(result.ModifiedAmount, Is.EqualTo(12.5f));
        Assert.That(result.AppliedAmount, Is.EqualTo(12.5f));
    }

    [Test]
    public void Resolve_DoesNotApplyCriticalDamageWhenRollFails()
    {
        FakeDamageSource source = new(criticalChance: 0.1f, criticalDamageMultiplier: 1.25f);

        DamageResolution result = DamageResolver.Resolve(
            new DamageRequest(10f, source, DamageOrigin.BasicAttack),
            currentHp: 20f,
            targetIsDead: false,
            randomValueProvider: () => 0.1f);

        Assert.That(result.IsCritical, Is.False);
        Assert.That(result.ModifiedAmount, Is.EqualTo(10f));
    }

    [Test]
    public void Resolve_ExcludesEffectDamageFromCriticalRoll()
    {
        FakeDamageSource source = new(criticalChance: 1f, criticalDamageMultiplier: 2f);

        DamageResolution result = DamageResolver.Resolve(
            new DamageRequest(10f, source, DamageOrigin.Effect),
            currentHp: 20f,
            targetIsDead: false,
            randomValueProvider: () => 0f);

        Assert.That(result.IsCritical, Is.False);
        Assert.That(result.ModifiedAmount, Is.EqualTo(10f));
    }

    private sealed class FakeDamageSource : ICombatTarget, ICombatDamageSource
    {
        public Transform TargetTransform => null;
        public ICombatHealth CombatHealth => null;
        public bool IsDead => false;
        public float CriticalChance { get; }
        public float CriticalDamageMultiplier { get; }

        public FakeDamageSource(float criticalChance, float criticalDamageMultiplier)
        {
            CriticalChance = criticalChance;
            CriticalDamageMultiplier = criticalDamageMultiplier;
        }
    }
}
