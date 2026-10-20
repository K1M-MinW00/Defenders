using System;

public sealed class FirstBattleTutorialSession
{
    public string CurrentStep { get; private set; } = FirstBattleTutorialSequence.InitialStep;

    public bool TryAdvance(string expectedStep)
    {
        if (!string.Equals(CurrentStep, expectedStep, StringComparison.Ordinal) ||
            !FirstBattleTutorialSequence.TryGetNext(CurrentStep, out string nextStep))
        {
            return false;
        }

        CurrentStep = nextStep ?? string.Empty;
        return true;
    }
}
