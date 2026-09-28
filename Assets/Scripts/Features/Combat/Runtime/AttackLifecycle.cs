public enum AttackPhase
{
    Idle,
    Windup,
    Hit
}

public sealed class AttackLifecycle
{
    public ICombatTarget Target { get; private set; }
    public AttackPhase Phase { get; private set; } = AttackPhase.Idle;
    public bool IsActive => Phase != AttackPhase.Idle;

    public bool TryBegin(ICombatTarget target)
    {
        if (target == null || IsActive)
            return false;

        Target = target;
        Phase = AttackPhase.Windup;
        return true;
    }

    public bool TryEnterHitPhase()
    {
        if (Phase != AttackPhase.Windup)
            return false;

        Phase = AttackPhase.Hit;
        return true;
    }

    public bool Complete()
    {
        if (!IsActive)
            return false;

        Reset();
        return true;
    }

    public bool Cancel()
    {
        if (!IsActive)
            return false;

        Reset();
        return true;
    }

    private void Reset()
    {
        Target = null;
        Phase = AttackPhase.Idle;
    }
}
