using NUnit.Framework;
using UnityEngine;

public sealed class BossStatusResistanceTests
{
    [Test]
    public void StunResistance_ReducesDuration()
    {
        BossStatusResistanceSettings resistance = new() { stun = 0.75f };

        bool applied = StatusEffectResistanceResolver.TryResolve(
            StatusEffectApplication.Stun(4f),
            resistance,
            out StatusEffectApplication resolved);

        Assert.That(applied, Is.True);
        Assert.That(resolved.Duration, Is.EqualTo(1f).Within(0.001f));
        Assert.That(resolved.Magnitude, Is.EqualTo(1f));
    }

    [Test]
    public void PoisonResistance_ReducesTickDamageWithoutChangingDuration()
    {
        BossStatusResistanceSettings resistance = new() { poison = 0.25f };

        bool applied = StatusEffectResistanceResolver.TryResolve(
            StatusEffectApplication.Poison(6f, 20f, tickInterval: 1f),
            resistance,
            out StatusEffectApplication resolved);

        Assert.That(applied, Is.True);
        Assert.That(resolved.Duration, Is.EqualTo(6f));
        Assert.That(resolved.Magnitude, Is.EqualTo(15f).Within(0.001f));
    }

    [Test]
    public void FullResistance_RejectsEffect()
    {
        BossStatusResistanceSettings resistance = new() { poison = 1f };

        Assert.That(StatusEffectResistanceResolver.TryResolve(
            StatusEffectApplication.Poison(6f, 20f),
            resistance,
            out _), Is.False);
    }

    [Test]
    public void IsImmune_OnlyReturnsTrueAtFullResistance()
    {
        Assert.That(StatusEffectResistanceResolver.IsImmune(
            StatusEffectType.Poison,
            new BossStatusResistanceSettings { poison = 1f }), Is.True);
        Assert.That(StatusEffectResistanceResolver.IsImmune(
            StatusEffectType.Poison,
            new BossStatusResistanceSettings { poison = 0.75f }), Is.False);
    }

    [Test]
    public void NoResistance_PreservesEffect()
    {
        StatusEffectApplication application = StatusEffectApplication.Slow(5f, 0.4f);

        Assert.That(StatusEffectResistanceResolver.TryResolve(application, null, out StatusEffectApplication resolved), Is.True);
        Assert.That(resolved.Duration, Is.EqualTo(5f));
        Assert.That(resolved.Magnitude, Is.EqualTo(0.4f).Within(0.001f));
    }
}
