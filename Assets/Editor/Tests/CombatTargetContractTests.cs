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
    public void HealthComponents_ExposeCommonHealthContract()
    {
        Assert.That(typeof(ICombatHealth).IsAssignableFrom(typeof(UnitHealth)), Is.True);
        Assert.That(typeof(ICombatHealth).IsAssignableFrom(typeof(MonsterHealth)), Is.True);
        Assert.That(typeof(IDamageable).IsAssignableFrom(typeof(ICombatHealth)), Is.True);
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
            if (damage <= 0f)
                return;

            CurrentHp = 0f;
            IsDead = true;
        }
    }
}
