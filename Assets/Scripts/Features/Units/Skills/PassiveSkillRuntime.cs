using UnityEngine;

public sealed class PassiveProcCounter
{
    public int Count { get; private set; }

    public bool Register(int requiredCount)
    {
        requiredCount = Mathf.Max(1, requiredCount);
        Count++;
        if (Count < requiredCount)
            return false;

        Count = 0;
        return true;
    }

    public void Reset() => Count = 0;
}

public sealed class PassiveCooldown
{
    private float readyAt = float.NegativeInfinity;

    public float Remaining(float now) => Mathf.Max(0f, readyAt - now);

    public bool TryConsume(float now, float duration)
    {
        if (now < readyAt)
            return false;

        readyAt = now + Mathf.Max(0f, duration);
        return true;
    }

    public void Reset() => readyAt = float.NegativeInfinity;
}

public sealed class PassiveStackCounter
{
    public int Count { get; private set; }

    public int Add(int maximum)
    {
        Count = Mathf.Min(Count + 1, Mathf.Max(1, maximum));
        return Count;
    }

    public void Reset() => Count = 0;
}
