using System;

public sealed class StagePhaseMachine
{
    public StageState Current { get; private set; } = StageState.None;

    public event Action<StageState, StageState> Changed;

    public bool CanTransitionTo(StageState next)
    {
        return Current switch
        {
            StageState.None => next == StageState.Loading,
            StageState.Loading => next == StageState.Preparing ||
                                  next == StageState.InitializationFailed,
            StageState.Preparing => next == StageState.Combat ||
                                    next == StageState.Resolving ||
                                    next == StageState.InitializationFailed,
            StageState.Combat => next == StageState.Resolving,
            StageState.Resolving => next == StageState.Preparing ||
                                    next == StageState.StageClear ||
                                    next == StageState.StageFail,
            _ => false,
        };
    }

    public bool TryTransition(StageState next)
    {
        if (!CanTransitionTo(next))
            return false;

        StageState previous = Current;
        Current = next;
        Changed?.Invoke(previous, next);
        return true;
    }
}
