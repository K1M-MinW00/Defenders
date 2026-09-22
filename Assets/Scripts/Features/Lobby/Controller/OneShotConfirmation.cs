using System;

public sealed class OneShotConfirmation
{
    private Action pendingAction;

    public bool HasPendingAction => pendingAction != null;

    public bool TryOpen(Action action)
    {
        if (action == null)
            return false;

        pendingAction = action;
        return true;
    }

    public bool TryConfirm()
    {
        Action action = Take();
        if (action == null)
            return false;

        action.Invoke();
        return true;
    }

    public void Cancel()
    {
        pendingAction = null;
    }

    private Action Take()
    {
        Action action = pendingAction;
        pendingAction = null;
        return action;
    }
}
