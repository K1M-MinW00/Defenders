public enum SkillExecutionPhase
{
    Idle,
    Prepared,
    Running,
    Applied
}

public sealed class SkillExecutionLifecycle
{
    public SkillExecutionContext Context { get; private set; }
    public SkillExecutionPhase Phase { get; private set; } = SkillExecutionPhase.Idle;
    public bool IsActive => Phase != SkillExecutionPhase.Idle;
    public bool IsRunning => Phase == SkillExecutionPhase.Running || Phase == SkillExecutionPhase.Applied;

    public bool TryPrepare(SkillExecutionContext context)
    {
        if (context == null || IsActive)
            return false;

        Context = context;
        Phase = SkillExecutionPhase.Prepared;
        return true;
    }

    public bool TryStart()
    {
        if (Phase != SkillExecutionPhase.Prepared)
            return false;

        Phase = SkillExecutionPhase.Running;
        return true;
    }

    public bool TryApply()
    {
        if (Phase != SkillExecutionPhase.Running)
            return false;

        Phase = SkillExecutionPhase.Applied;
        return true;
    }

    public bool Complete()
    {
        if (!IsRunning)
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
        Context = null;
        Phase = SkillExecutionPhase.Idle;
    }
}
