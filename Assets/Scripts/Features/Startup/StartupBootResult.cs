public sealed class StartupBootResult
{
    public bool Succeeded { get; }
    public StartupBootFailure Failure { get; }
    public string ErrorMessage { get; }

    private StartupBootResult(bool succeeded, StartupBootFailure failure, string errorMessage)
    {
        Succeeded = succeeded;
        Failure = failure;
        ErrorMessage = errorMessage;
    }

    public static StartupBootResult Success() => new(true, StartupBootFailure.None, null);

    public static StartupBootResult Fail(StartupBootFailure failure, string errorMessage)
        => new(false, failure, errorMessage);
}
