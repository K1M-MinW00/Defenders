using System;

public sealed class StageRerollAllowance
{
    public int Remaining { get; private set; }
    public bool HasFreeReroll => Remaining > 0;
    public event Action<int> Changed;

    public void Grant(int count)
    {
        if (count <= 0)
            return;

        Remaining += count;
        Changed?.Invoke(Remaining);
    }

    public bool TryConsume()
    {
        if (Remaining <= 0)
            return false;

        Remaining--;
        Changed?.Invoke(Remaining);
        return true;
    }
}
