using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public sealed class UnitStatusEffectControllerTests
{
    [Test]
    public void Slow_AppliesStrongestValueAndClearRestoresSpeed()
    {
        GameObject gameObject = new("UnitStatusTest");
        NavMeshAgent agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = 4f;
        UnitMovementController movement = gameObject.AddComponent<UnitMovementController>();
        UnitStatusEffectController effects = gameObject.AddComponent<UnitStatusEffectController>();
        UnitController unit = gameObject.AddComponent<UnitController>();
        movement.Initialize(unit);
        effects.Initialize(unit);

        Assert.That(effects.Apply(StatusEffectApplication.Slow(3f, 0.25f)), Is.True);
        Assert.That(agent.speed, Is.EqualTo(3f).Within(0.001f));

        effects.Apply(StatusEffectApplication.Slow(1f, 0.1f));
        Assert.That(agent.speed, Is.EqualTo(3f).Within(0.001f));
        Assert.That(effects.TryGet(StatusEffectType.Slow, out UnitStatusEffectController.Snapshot snapshot), Is.True);
        Assert.That(snapshot.RemainingTime, Is.EqualTo(3f).Within(0.001f));
        Assert.That(snapshot.Magnitude, Is.EqualTo(0.25f).Within(0.001f));

        effects.ClearAll();
        Assert.That(agent.speed, Is.EqualTo(4f).Within(0.001f));
        Assert.That(effects.ActiveEffects, Is.Empty);

        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void Stun_ExpiresAfterDurationAndDoesNotStackAdditively()
    {
        GameObject gameObject = new("UnitStunTest");
        gameObject.AddComponent<NavMeshAgent>();
        gameObject.AddComponent<UnitMovementController>();
        UnitStatusEffectController effects = gameObject.AddComponent<UnitStatusEffectController>();
        UnitController unit = gameObject.AddComponent<UnitController>();
        effects.Initialize(unit);

        Assert.That(effects.Apply(StatusEffectApplication.Stun(1.25f)), Is.True);
        Assert.That(effects.IsStunned, Is.True);

        effects.Tick(0.5f);
        effects.Apply(StatusEffectApplication.Stun(0.5f));
        Assert.That(effects.TryGet(StatusEffectType.Stun, out UnitStatusEffectController.Snapshot snapshot), Is.True);
        Assert.That(snapshot.RemainingTime, Is.EqualTo(0.75f).Within(0.001f));

        effects.Tick(0.76f);
        Assert.That(effects.IsStunned, Is.False);
        Assert.That(effects.TryGet(StatusEffectType.Stun, out _), Is.False);

        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void Poison_TicksAtIntervalAndRefreshDoesNotCauseImmediateTick()
    {
        GameObject gameObject = new("UnitPoisonTest");
        gameObject.AddComponent<NavMeshAgent>();
        gameObject.AddComponent<UnitMovementController>();
        UnitStatusEffectController effects = gameObject.AddComponent<UnitStatusEffectController>();
        UnitController unit = gameObject.AddComponent<UnitController>();
        effects.Initialize(unit);

        int tickCount = 0;
        float totalDamage = 0f;
        effects.PeriodicEffectTicked += (type, damage) =>
        {
            Assert.That(type, Is.EqualTo(StatusEffectType.Poison));
            tickCount++;
            totalDamage += damage;
        };

        Assert.That(effects.Apply(StatusEffectApplication.Poison(3f, 10f)), Is.True);
        effects.Tick(0.99f);
        Assert.That(tickCount, Is.Zero);

        effects.Apply(StatusEffectApplication.Poison(1f, 5f));
        Assert.That(tickCount, Is.Zero);

        effects.Tick(0.02f);
        Assert.That(tickCount, Is.EqualTo(1));
        Assert.That(totalDamage, Is.EqualTo(10f).Within(0.001f));

        effects.Tick(2f);
        Assert.That(tickCount, Is.EqualTo(3));
        Assert.That(totalDamage, Is.EqualTo(30f).Within(0.001f));
        Assert.That(effects.TryGet(StatusEffectType.Poison, out _), Is.False);

        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void Burn_TicksIndependentlyFromPoisonAtFasterInterval()
    {
        GameObject gameObject = new("UnitBurnTest");
        gameObject.AddComponent<NavMeshAgent>();
        gameObject.AddComponent<UnitMovementController>();
        UnitStatusEffectController effects = gameObject.AddComponent<UnitStatusEffectController>();
        UnitController unit = gameObject.AddComponent<UnitController>();
        effects.Initialize(unit);

        int poisonTicks = 0;
        int burnTicks = 0;
        effects.PeriodicEffectTicked += (type, _) =>
        {
            if (type == StatusEffectType.Poison)
                poisonTicks++;
            if (type == StatusEffectType.Burn)
                burnTicks++;
        };

        effects.Apply(StatusEffectApplication.Poison(1f, 10f));
        effects.Apply(StatusEffectApplication.Burn(1f, 6f));
        effects.Tick(0.5f);

        Assert.That(poisonTicks, Is.Zero);
        Assert.That(burnTicks, Is.EqualTo(1));
        Assert.That(effects.ActiveEffects.Count, Is.EqualTo(2));

        effects.Tick(0.5f);
        Assert.That(poisonTicks, Is.EqualTo(1));
        Assert.That(burnTicks, Is.EqualTo(2));
        Assert.That(effects.ActiveEffects, Is.Empty);

        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void DefenseDown_UsesStrongestPenaltyAndExpires()
    {
        GameObject gameObject = new("UnitDefenseDownTest");
        gameObject.AddComponent<NavMeshAgent>();
        gameObject.AddComponent<UnitMovementController>();
        UnitStatusEffectController effects = gameObject.AddComponent<UnitStatusEffectController>();
        UnitController unit = gameObject.AddComponent<UnitController>();
        effects.Initialize(unit);

        effects.Apply(StatusEffectApplication.DefenseDown(5f, 20f));
        effects.Apply(StatusEffectApplication.DefenseDown(2f, 10f));

        Assert.That(effects.DefenseReductionPenalty, Is.EqualTo(20f));
        Assert.That(UnitHealth.ResolveDamageReduction(35f, effects.DefenseReductionPenalty), Is.EqualTo(15f));
        Assert.That(UnitHealth.ResolveDamageReduction(10f, effects.DefenseReductionPenalty), Is.Zero);
        Assert.That(effects.TryGet(StatusEffectType.DefenseDown, out UnitStatusEffectController.Snapshot snapshot), Is.True);
        Assert.That(snapshot.RemainingTime, Is.EqualTo(5f));

        effects.Tick(5f);
        Assert.That(effects.DefenseReductionPenalty, Is.Zero);

        Object.DestroyImmediate(gameObject);
    }
}
