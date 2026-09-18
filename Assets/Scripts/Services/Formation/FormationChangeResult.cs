public enum FormationChangeFailure
{
    None,
    InvalidRequest,
    InvalidRoster,
    UnitNotOwned,
    UnitNotSelected,
    UnitAlreadySelected,
    SaveFailed,
}

public sealed class FormationChangeResult
{
    public bool Succeeded => Failure == FormationChangeFailure.None;
    public FormationChangeFailure Failure { get; }

    private FormationChangeResult(FormationChangeFailure failure)
    {
        Failure = failure;
    }

    public static FormationChangeResult Success() => new(FormationChangeFailure.None);
    public static FormationChangeResult Fail(FormationChangeFailure failure) => new(failure);
}
