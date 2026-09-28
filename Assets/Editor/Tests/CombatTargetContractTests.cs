using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class CombatTargetContractTests
{
    [Test]
    public void Controllers_ExposeCommonCombatTargetContract()
    {
        Assert.That(typeof(ICombatTarget).IsAssignableFrom(typeof(UnitController)), Is.True);
        Assert.That(typeof(ICombatTarget).IsAssignableFrom(typeof(MonsterController)), Is.True);
    }

    [Test]
    public void MonsterTargeting_StartsWithoutAValidTarget()
    {
        GameObject gameObject = new("MonsterTargeting");

        try
        {
            MonsterTargetingController targeting = gameObject.AddComponent<MonsterTargetingController>();
            targeting.Initialize(null);

            Assert.That(targeting.CurrentTarget, Is.Null);
            Assert.That(targeting.HasValidTarget(), Is.False);
            Assert.That(targeting.TryAcquireClosest(Vector3.zero), Is.False);
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void HealthComponents_ExposeCommonHealthContract()
    {
        Assert.That(typeof(ICombatHealth).IsAssignableFrom(typeof(UnitHealth)), Is.True);
        Assert.That(typeof(ICombatHealth).IsAssignableFrom(typeof(MonsterHealth)), Is.True);
        Assert.That(typeof(IDamageable).IsAssignableFrom(typeof(ICombatHealth)), Is.True);
    }

    [Test]
    public void DamageRequest_ReturnsAppliedAndLethalResult()
    {
        FakeHealth health = new(false);
        DamageRequest request = new(2f, origin: DamageOrigin.BasicAttack);

        DamageResult result = health.ApplyDamage(request);

        Assert.That(result.RequestedAmount, Is.EqualTo(2f));
        Assert.That(result.AppliedAmount, Is.EqualTo(1f));
        Assert.That(result.WasApplied, Is.True);
        Assert.That(result.WasLethal, Is.True);
        Assert.That(result.RejectReason, Is.EqualTo(DamageRejectReason.None));
    }

    [Test]
    public void DamageRequest_RejectsDamageWhenTargetIsAlreadyDead()
    {
        FakeHealth health = new(true);

        DamageResult result = health.ApplyDamage(new DamageRequest(1f));

        Assert.That(result.WasApplied, Is.False);
        Assert.That(result.RejectReason, Is.EqualTo(DamageRejectReason.TargetAlreadyDead));
    }

    [Test]
    public void Selector_ReturnsClosestLivingTargetAndChecksRange()
    {
        GameObject nearObject = new("Near");
        GameObject farObject = new("Far");
        FakeTarget near = new(nearObject.transform, false);
        FakeTarget far = new(farObject.transform, false);
        FakeTarget dead = new(farObject.transform, true);
        nearObject.transform.position = Vector3.right;
        farObject.transform.position = Vector3.right * 3f;

        try
        {
            FakeTarget result = CombatTargetSelector.FindClosest(
                new List<FakeTarget> { far, dead, near },
                Vector3.zero);

            Assert.That(result, Is.SameAs(near));
            Assert.That(CombatTargetSelector.IsWithinRange(near, Vector3.zero, 1f), Is.True);
            Assert.That(CombatTargetSelector.IsWithinRange(far, Vector3.zero, 1f), Is.False);
            Assert.That(CombatTargetSelector.IsValid(dead), Is.False);
        }
        finally
        {
            Object.DestroyImmediate(nearObject);
            Object.DestroyImmediate(farObject);
        }
    }

    private sealed class FakeTarget : ICombatTarget
    {
        public Transform TargetTransform { get; }
        public ICombatHealth CombatHealth { get; }
        public bool IsDead => CombatHealth.IsDead;

        public FakeTarget(Transform targetTransform, bool isDead)
        {
            TargetTransform = targetTransform;
            CombatHealth = new FakeHealth(isDead);
        }
    }

    private sealed class FakeHealth : ICombatHealth
    {
        public float CurrentHp { get; private set; }
        public float MaxHp => 1f;
        public bool IsDead { get; private set; }

        public FakeHealth(bool isDead)
        {
            CurrentHp = isDead ? 0f : 1f;
            IsDead = isDead;
        }

        public void TakeDamage(float damage)
        {
            ApplyDamage(new DamageRequest(damage));
        }

        public DamageResult ApplyDamage(DamageRequest request)
        {
            if (IsDead)
                return DamageResult.Rejected(request.Amount, DamageRejectReason.TargetAlreadyDead);

            if (request.Amount <= 0f)
                return DamageResult.Rejected(request.Amount, DamageRejectReason.InvalidAmount);

            float appliedDamage = Mathf.Min(CurrentHp, request.Amount);
            CurrentHp = 0f;
            IsDead = true;
            return DamageResult.Applied(request.Amount, appliedDamage, true);
        }
    }
}
