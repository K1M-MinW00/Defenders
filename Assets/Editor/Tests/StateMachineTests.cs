using NUnit.Framework;

public sealed class StateMachineTests
{
    [Test]
    public void ChangeState_ExitsCurrentAndEntersNext()
    {
        StateMachine machine = new();
        FakeState first = new();
        FakeState second = new();

        machine.ChangeState(first);
        machine.ChangeState(second);

        Assert.That(first.EnterCount, Is.EqualTo(1));
        Assert.That(first.ExitCount, Is.EqualTo(1));
        Assert.That(second.EnterCount, Is.EqualTo(1));
        Assert.That(machine.CurrentState, Is.SameAs(second));
    }

    [Test]
    public void ChangeState_IgnoresSameStateInstance()
    {
        StateMachine machine = new();
        FakeState state = new();

        machine.ChangeState(state);
        machine.ChangeState(state);

        Assert.That(state.EnterCount, Is.EqualTo(1));
        Assert.That(state.ExitCount, Is.Zero);
    }

    [Test]
    public void Tick_UpdatesOnlyCurrentState()
    {
        StateMachine machine = new();
        FakeState state = new();

        machine.Tick();
        machine.ChangeState(state);
        machine.Tick();

        Assert.That(state.UpdateCount, Is.EqualTo(1));
    }

    [Test]
    public void Reset_ExitsAndAllowsSameStateToEnterAgain()
    {
        StateMachine machine = new();
        FakeState state = new();

        machine.ChangeState(state);
        machine.Reset();
        machine.ChangeState(state);

        Assert.That(state.EnterCount, Is.EqualTo(2));
        Assert.That(state.ExitCount, Is.EqualTo(1));
    }

    private sealed class FakeState : IState
    {
        public int EnterCount { get; private set; }
        public int UpdateCount { get; private set; }
        public int ExitCount { get; private set; }

        public void Enter() => EnterCount++;
        public void Update() => UpdateCount++;
        public void Exit() => ExitCount++;
    }
}
