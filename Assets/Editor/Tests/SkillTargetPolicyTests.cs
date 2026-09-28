using NUnit.Framework;
using UnityEngine;

public sealed class SkillTargetPolicyTests
{
    [Test]
    public void EnemySkill_RequiresLivingTargetAtApplyTime()
    {
        GameObject targetObject = new("Target");

        try
        {
            SkillExecutionContext context = new();
            context.SetEnemyTarget(new FakeTarget(targetObject.transform, false));

            Assert.That(SkillTargetPolicy.CanApply(ActiveSkillTargetType.EnemyInRange, context), Is.True);

            context.SetEnemyTarget(new FakeTarget(targetObject.transform, true));

            Assert.That(SkillTargetPolicy.CanApply(ActiveSkillTargetType.EnemyInRange, context), Is.False);
        }
        finally
        {
            Object.DestroyImmediate(targetObject);
        }
    }

    [Test]
    public void SelfAreaSkill_UsesLockedCastContextWithoutEnemy()
    {
        SkillExecutionContext context = new();
        context.SetCastPosition(Vector3.one);

        Assert.That(SkillTargetPolicy.CanApply(ActiveSkillTargetType.SelfArea, context), Is.True);
    }

    [Test]
    public void InvalidContext_IsRejected()
    {
        Assert.That(
            SkillTargetPolicy.CanApply(ActiveSkillTargetType.EnemyInRange, new SkillExecutionContext()),
            Is.False);
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
