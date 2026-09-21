public enum NicknameChangeFailure
{
    None,
    InvalidNickname,
    NoChange,
    InsufficientGem,
    NicknameAlreadyTaken,
    SaveFailed,
    Busy,
}

public sealed class NicknameChangeResult
{
    public bool Succeeded => Failure == NicknameChangeFailure.None;
    public NicknameChangeFailure Failure { get; }
    public int GemCost { get; }

    private NicknameChangeResult(NicknameChangeFailure failure, int gemCost)
    {
        Failure = failure;
        GemCost = gemCost;
    }

    public static NicknameChangeResult Success(int gemCost) => new(NicknameChangeFailure.None, gemCost);
    public static NicknameChangeResult Fail(NicknameChangeFailure failure, int gemCost = 0) => new(failure, gemCost);
}
