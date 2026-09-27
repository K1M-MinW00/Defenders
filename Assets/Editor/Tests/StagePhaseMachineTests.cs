using NUnit.Framework;

public class StagePhaseMachineTests
{
    [Test]
    public void CompleteStage_FollowsAllowedLifecycle()
    {
        var machine = new StagePhaseMachine();

        Assert.That(machine.TryTransition(StageState.Loading), Is.True);
        Assert.That(machine.TryTransition(StageState.Preparing), Is.True);
        Assert.That(machine.TryTransition(StageState.Combat), Is.True);
        Assert.That(machine.TryTransition(StageState.WaveCleared), Is.True);
        Assert.That(machine.TryTransition(StageState.Preparing), Is.True);
        Assert.That(machine.TryTransition(StageState.Combat), Is.True);
        Assert.That(machine.TryTransition(StageState.WaveCleared), Is.True);
        Assert.That(machine.TryTransition(StageState.StageClear), Is.True);
        Assert.That(machine.Current, Is.EqualTo(StageState.StageClear));
    }

    [Test]
    public void InvalidTransition_IsRejectedWithoutChangingState()
    {
        var machine = new StagePhaseMachine();

        Assert.That(machine.TryTransition(StageState.Combat), Is.False);
        Assert.That(machine.Current, Is.EqualTo(StageState.None));

        Assert.That(machine.TryTransition(StageState.Loading), Is.True);
        Assert.That(machine.TryTransition(StageState.StageClear), Is.False);
        Assert.That(machine.Current, Is.EqualTo(StageState.Loading));
    }

    [Test]
    public void Failure_CanExitFromPreparationOrCombat()
    {
        var preparingFailure = new StagePhaseMachine();
        Assert.That(preparingFailure.TryTransition(StageState.Loading), Is.True);
        Assert.That(preparingFailure.TryTransition(StageState.Preparing), Is.True);
        Assert.That(preparingFailure.TryTransition(StageState.StageFail), Is.True);

        var combatFailure = new StagePhaseMachine();
        Assert.That(combatFailure.TryTransition(StageState.Loading), Is.True);
        Assert.That(combatFailure.TryTransition(StageState.Preparing), Is.True);
        Assert.That(combatFailure.TryTransition(StageState.Combat), Is.True);
        Assert.That(combatFailure.TryTransition(StageState.StageFail), Is.True);
    }

    [Test]
    public void ChangedEvent_ReportsPreviousAndCurrentPhase()
    {
        var machine = new StagePhaseMachine();
        StageState previous = StageState.StageFail;
        StageState current = StageState.StageFail;
        machine.Changed += (from, to) =>
        {
            previous = from;
            current = to;
        };

        Assert.That(machine.TryTransition(StageState.Loading), Is.True);
        Assert.That(previous, Is.EqualTo(StageState.None));
        Assert.That(current, Is.EqualTo(StageState.Loading));
    }
}
