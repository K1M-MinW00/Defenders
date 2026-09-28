using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class ProjectileHitResolverTests
{
    [Test]
    public void Resolve_AcceptsLivingTargetOnlyOnce()
    {
        GameObject targetObject = new("Target");
        FakeTarget target = new(targetObject.transform, false);
        var damagedTargets = new HashSet<ICombatHealth>();

        try
        {
            Assert.That(
                ProjectileHitResolver.TryResolve(target, damagedTargets, out ICombatHealth health),
                Is.True);
            Assert.That(health, Is.SameAs(target.CombatHealth));
            Assert.That(
                ProjectileHitResolver.TryResolve(target, damagedTargets, out _),
                Is.False);
        }
        finally
        {
            Object.DestroyImmediate(targetObject);
        }
    }

    [Test]
    public void Resolve_RejectsInactiveOrDeadTarget()
    {
        GameObject inactiveObject = new("Inactive");
        GameObject deadObject = new("Dead");
        FakeTarget inactive = new(inactiveObject.transform, false);
        FakeTarget dead = new(deadObject.transform, true);

        try
        {
            inactiveObject.SetActive(false);

            Assert.That(ProjectileHitResolver.TryResolve(inactive, null, out _), Is.False);
            Assert.That(ProjectileHitResolver.TryResolve(dead, null, out _), Is.False);
        }
        finally
        {
            Object.DestroyImmediate(inactiveObject);
            Object.DestroyImmediate(deadObject);
        }
    }

    private sealed class FakeTarget : ICombatTarget
    {
        public Transform TargetTransform { get; }
        public ICombatHealth CombatHealth { get; }
        public bool IsDead => CombatHealth.IsDead;

        public FakeTarget(Transform transform, bool isDead)
        {
            TargetTransform = transform;
            CombatHealth = new FakeHealth(isDead);
        }
    }

    private sealed class FakeHealth : ICombatHealth
    {
        public float CurrentHp => IsDead ? 0f : 1f;
        public float MaxHp => 1f;
        public bool IsDead { get; }

        public FakeHealth(bool isDead)
        {
            IsDead = isDead;
        }

        public void TakeDamage(float damage) { }

        public DamageResult ApplyDamage(DamageRequest request)
        {
            return IsDead
                ? DamageResult.Rejected(request.Amount, DamageRejectReason.TargetAlreadyDead)
                : DamageResult.Applied(request.Amount, request.Amount, false);
        }
    }
}
