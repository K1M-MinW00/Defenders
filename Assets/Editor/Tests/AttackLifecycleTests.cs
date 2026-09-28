using NUnit.Framework;

public class AttackLifecycleTests
{
    private sealed class TestTarget : ICombatTarget
    {
        public UnityEngine.Transform TargetTransform => null;
        public ICombatHealth CombatHealth => null;
        public bool IsDead => false;
    }

    [Test]
    public void NewLifecycle_IsIdle()
    {
        var lifecycle = new AttackLifecycle();

        Assert.That(lifecycle.Phase, Is.EqualTo(AttackPhase.Idle));
        Assert.That(lifecycle.IsActive, Is.False);
        Assert.That(lifecycle.Target, Is.Null);
    }

    [Test]
    public void BeginHitComplete_TransitionsInOrder()
    {
        var lifecycle = new AttackLifecycle();
        var target = new TestTarget();

        Assert.That(lifecycle.TryBegin(target), Is.True);
        Assert.That(lifecycle.Phase, Is.EqualTo(AttackPhase.Windup));
        Assert.That(lifecycle.Target, Is.SameAs(target));
        Assert.That(lifecycle.TryEnterHitPhase(), Is.True);
        Assert.That(lifecycle.Phase, Is.EqualTo(AttackPhase.Hit));
        Assert.That(lifecycle.Complete(), Is.True);
        Assert.That(lifecycle.Phase, Is.EqualTo(AttackPhase.Idle));
        Assert.That(lifecycle.Target, Is.Null);
    }

    [Test]
    public void DuplicateBeginAndHit_AreRejected()
    {
        var lifecycle = new AttackLifecycle();
        var target = new TestTarget();

        Assert.That(lifecycle.TryBegin(target), Is.True);
        Assert.That(lifecycle.TryBegin(target), Is.False);
        Assert.That(lifecycle.TryEnterHitPhase(), Is.True);
        Assert.That(lifecycle.TryEnterHitPhase(), Is.False);
    }

    [Test]
    public void Cancel_ClearsActiveAttack()
    {
        var lifecycle = new AttackLifecycle();
        lifecycle.TryBegin(new TestTarget());

        Assert.That(lifecycle.Cancel(), Is.True);
        Assert.That(lifecycle.IsActive, Is.False);
        Assert.That(lifecycle.TryEnterHitPhase(), Is.False);
        Assert.That(lifecycle.Complete(), Is.False);
    }
}
