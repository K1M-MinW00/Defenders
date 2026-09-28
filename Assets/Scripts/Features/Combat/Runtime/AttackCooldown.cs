public sealed class AttackCooldown
{
    private float readyAt = float.NegativeInfinity;

    public bool IsReady(float currentTime)
    {
        return currentTime >= readyAt;
    }

    public float GetRemaining(float currentTime)
    {
        float remaining = readyAt - currentTime;
        return remaining > 0f ? remaining : 0f;
    }

    public void Start(float currentTime, float duration)
    {
        if (float.IsNaN(duration) || float.IsInfinity(duration) || duration < 0f)
            duration = 0f;

        readyAt = currentTime + duration;
    }

    public void Reset()
    {
        readyAt = float.NegativeInfinity;
    }
}
