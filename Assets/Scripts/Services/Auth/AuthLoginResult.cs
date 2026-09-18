public sealed class AuthLoginResult
{
    public bool Succeeded { get; }
    public string UserId { get; }
    public string ErrorMessage { get; }

    private AuthLoginResult(bool succeeded, string userId, string errorMessage)
    {
        Succeeded = succeeded;
        UserId = userId;
        ErrorMessage = errorMessage;
    }

    public static AuthLoginResult Success(string userId)
    {
        return new AuthLoginResult(true, userId, null);
    }

    public static AuthLoginResult Fail(string errorMessage)
    {
        return new AuthLoginResult(false, null, errorMessage);
    }
}
