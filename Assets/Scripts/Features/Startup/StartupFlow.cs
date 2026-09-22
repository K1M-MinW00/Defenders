using System;
using System.Threading.Tasks;

public sealed class StartupFlow
{
    private readonly Action initializeLocalData;
    private readonly Func<Task<AuthLoginResult>> signInAsync;
    private readonly Func<string, Task<bool>> loadUserDataAsync;

    public StartupFlow(Action initializeLocalData, Func<Task<AuthLoginResult>> signInAsync, Func<string, Task<bool>> loadUserDataAsync)
    {
        this.initializeLocalData = initializeLocalData ?? throw new ArgumentNullException(nameof(initializeLocalData));
        this.signInAsync = signInAsync ?? throw new ArgumentNullException(nameof(signInAsync));
        this.loadUserDataAsync = loadUserDataAsync ?? throw new ArgumentNullException(nameof(loadUserDataAsync));
    }

    public async Task<StartupBootResult> RunAsync(Action<StartupBootStage> onStageChanged = null)
    {
        try
        {
            onStageChanged?.Invoke(StartupBootStage.Initializing);
            initializeLocalData();

            onStageChanged?.Invoke(StartupBootStage.SigningIn);
            AuthLoginResult loginResult = await signInAsync();
            if (loginResult == null || !loginResult.Succeeded || string.IsNullOrWhiteSpace(loginResult.UserId))
                return StartupBootResult.Fail(StartupBootFailure.LoginFailed, loginResult?.ErrorMessage ?? "Login failed.");

            onStageChanged?.Invoke(StartupBootStage.LoadingUserData);
            if (!await loadUserDataAsync(loginResult.UserId))
                return StartupBootResult.Fail(StartupBootFailure.UserDataLoadFailed, "User data load failed.");

            onStageChanged?.Invoke(StartupBootStage.Ready);
            return StartupBootResult.Success();
        }
        catch (Exception exception)
        {
            return StartupBootResult.Fail(StartupBootFailure.UnexpectedError, exception.ToString());
        }
    }
}
