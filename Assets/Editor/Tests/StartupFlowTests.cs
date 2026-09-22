using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class StartupFlowTests
{
    public async Task RunAsync_CompletesStagesInOrder()
    {
        bool initialized = false;
        string loadedUserId = null;
        List<StartupBootStage> stages = new();
        StartupFlow flow = new(
            () => initialized = true,
            () => Task.FromResult(AuthLoginResult.Success("user-1")),
            userId =>
            {
                loadedUserId = userId;
                return Task.FromResult(true);
            });

        StartupBootResult result = await flow.RunAsync(stages.Add);

        Assert(result.Succeeded, "Startup flow should succeed.");
        Assert(initialized, "Local data should be initialized.");
        Assert(loadedUserId == "user-1", "Authenticated user id should be loaded.");
        Assert(stages.Count == 4, "All startup stages should be reported.");
        Assert(stages[0] == StartupBootStage.Initializing && stages[3] == StartupBootStage.Ready, "Startup stages are out of order.");
    }

    public async Task RunAsync_StopsWhenLoginFails()
    {
        bool loadCalled = false;
        StartupFlow flow = new(
            () => { },
            () => Task.FromResult(AuthLoginResult.Fail("failed")),
            _ =>
            {
                loadCalled = true;
                return Task.FromResult(true);
            });

        StartupBootResult result = await flow.RunAsync();

        Assert(!result.Succeeded && result.Failure == StartupBootFailure.LoginFailed, "Login failure should be returned.");
        Assert(!loadCalled, "User data load must not run after login failure.");
    }

    public async Task RunAsync_ReturnsUnexpectedError()
    {
        StartupFlow flow = new(
            () => throw new InvalidOperationException("test"),
            () => Task.FromResult(AuthLoginResult.Success("user-1")),
            _ => Task.FromResult(true));

        StartupBootResult result = await flow.RunAsync();

        Assert(!result.Succeeded && result.Failure == StartupBootFailure.UnexpectedError, "Unexpected exception should be converted to a failure result.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
