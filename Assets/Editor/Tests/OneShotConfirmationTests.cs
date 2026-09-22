using System;

public sealed class OneShotConfirmationTests
{
    public void TryConfirm_InvokesActionOnlyOnce()
    {
        OneShotConfirmation confirmation = new();
        int invocationCount = 0;

        Assert(confirmation.TryOpen(() => invocationCount++), "A valid confirmation action should open.");
        Assert(confirmation.HasPendingAction, "The opened action should be pending.");
        Assert(confirmation.TryConfirm(), "The pending action should be confirmed.");
        Assert(!confirmation.TryConfirm(), "A consumed action must not be confirmed twice.");
        Assert(invocationCount == 1, "The confirmation action should run exactly once.");
    }

    public void Cancel_ClearsPendingAction()
    {
        OneShotConfirmation confirmation = new();
        int invocationCount = 0;

        confirmation.TryOpen(() => invocationCount++);
        confirmation.Cancel();

        Assert(!confirmation.HasPendingAction, "Cancel should clear the pending action.");
        Assert(!confirmation.TryConfirm(), "A cancelled action must not be confirmed.");
        Assert(invocationCount == 0, "A cancelled action must not run.");
        Assert(!confirmation.TryOpen(null), "A null action must be rejected.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
