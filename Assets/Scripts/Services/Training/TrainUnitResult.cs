public enum TrainUnitFailure
{
    None,
    InvalidRequest,
    UnitNotFound,
    MaxLevel,
    InsufficientGold,
    InsufficientMaterials,
    SaveFailed,
}

public sealed class TrainUnitResult
{
    public bool Succeeded => Failure == TrainUnitFailure.None;
    public TrainUnitFailure Failure { get; }
    public int Level { get; }
    public int Exp { get; }

    private TrainUnitResult(TrainUnitFailure failure, int level = 0, int exp = 0)
    {
        Failure = failure;
        Level = level;
        Exp = exp;
    }

    public static TrainUnitResult Success(int level, int exp) =>
        new(TrainUnitFailure.None, level, exp);

    public static TrainUnitResult Fail(TrainUnitFailure failure) =>
        new(failure);
}
