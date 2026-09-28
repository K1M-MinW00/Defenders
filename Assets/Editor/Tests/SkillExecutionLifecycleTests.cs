using NUnit.Framework;

public sealed class SkillExecutionLifecycleTests
{
    [Test]
    public void NewLifecycle_IsIdle()
    {
        SkillExecutionLifecycle lifecycle = new();

        Assert.That(lifecycle.Phase, Is.EqualTo(SkillExecutionPhase.Idle));
        Assert.That(lifecycle.IsActive, Is.False);
        Assert.That(lifecycle.IsRunning, Is.False);
        Assert.That(lifecycle.Context, Is.Null);
    }

    [Test]
    public void PrepareStartApplyComplete_TransitionsInOrder()
    {
        SkillExecutionLifecycle lifecycle = new();
        SkillExecutionContext context = new();

        Assert.That(lifecycle.TryPrepare(context), Is.True);
        Assert.That(lifecycle.Phase, Is.EqualTo(SkillExecutionPhase.Prepared));
        Assert.That(lifecycle.TryStart(), Is.True);
        Assert.That(lifecycle.Phase, Is.EqualTo(SkillExecutionPhase.Running));
        Assert.That(lifecycle.TryApply(), Is.True);
        Assert.That(lifecycle.Phase, Is.EqualTo(SkillExecutionPhase.Applied));
        Assert.That(lifecycle.Complete(), Is.True);
        Assert.That(lifecycle.Phase, Is.EqualTo(SkillExecutionPhase.Idle));
        Assert.That(lifecycle.Context, Is.Null);
    }

    [Test]
    public void DuplicateTransitions_AreRejected()
    {
        SkillExecutionLifecycle lifecycle = new();
        SkillExecutionContext context = new();

        Assert.That(lifecycle.TryPrepare(context), Is.True);
        Assert.That(lifecycle.TryPrepare(context), Is.False);
        Assert.That(lifecycle.TryStart(), Is.True);
        Assert.That(lifecycle.TryStart(), Is.False);
        Assert.That(lifecycle.TryApply(), Is.True);
        Assert.That(lifecycle.TryApply(), Is.False);
        Assert.That(lifecycle.Complete(), Is.True);
        Assert.That(lifecycle.Complete(), Is.False);
    }

    [Test]
    public void Cancel_ClearsPreparedAndRunningSkill()
    {
        SkillExecutionLifecycle lifecycle = new();

        lifecycle.TryPrepare(new SkillExecutionContext());
        Assert.That(lifecycle.Cancel(), Is.True);
        Assert.That(lifecycle.IsActive, Is.False);

        lifecycle.TryPrepare(new SkillExecutionContext());
        lifecycle.TryStart();
        Assert.That(lifecycle.Cancel(), Is.True);
        Assert.That(lifecycle.IsRunning, Is.False);
        Assert.That(lifecycle.Context, Is.Null);
    }
}
