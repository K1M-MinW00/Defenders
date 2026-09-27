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

    public void SetMaximum(float max, bool restoreFull)
    {
        float nextMax = Mathf.Max(0f, max);
        if (restoreFull)
        {
            Initialize(nextMax);
            return;
        }

        float ratio = Max > 0f ? Current / Max : 1f;
        Max = nextMax;
        Current = IsDead ? 0f : Mathf.Clamp(Max * ratio, 0f, Max);
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
