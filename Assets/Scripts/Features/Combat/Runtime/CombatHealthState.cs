using UnityEngine;

public sealed class CombatHealthState
{
    public float Current { get; private set; }
    public float Max { get; private set; }
    public bool IsDead { get; private set; }

    public void Initialize(float max)
    {
        Max = Mathf.Max(0f, max);
        RestoreFull();
    }

    public void RestoreFull()
    {
        Current = Max;
        IsDead = false;
    }

    public float TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f)
            return 0f;

        float applied = Mathf.Min(amount, Current);
        Current -= applied;
        if (Current <= 0f)
            IsDead = true;

        return applied;
    }

    public float Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return 0f;

        float previous = Current;
        Current = Mathf.Min(Max, Current + amount);
        return Current - previous;
    }

    public bool Kill()
    {
        if (IsDead)
            return false;

        Current = 0f;
        IsDead = true;
        return true;
    }
}
