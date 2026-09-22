using System;
using System.Threading.Tasks;

public sealed class SceneFlowServiceTests
{
    public async Task LoadAsync_CompletesValidScene()
    {
        FakeSceneLoader loader = new(canLoad: true);
        SceneFlowService service = new(loader);

        SceneTransitionResult result = await service.LoadAsync("LobbyScene");

        Assert(result == SceneTransitionResult.Succeeded, "Valid scene should load successfully.");
        Assert(loader.LoadCount == 1, "Scene loader should run once.");
        Assert(!service.IsLoading, "Loading state should reset after success.");
    }

    public async Task LoadAsync_RejectsConcurrentTransition()
    {
        TaskCompletionSource<bool> completion = new();
        FakeSceneLoader loader = new(canLoad: true, completion.Task);
        SceneFlowService service = new(loader);

        Task<SceneTransitionResult> first = service.LoadAsync("LobbyScene");
        SceneTransitionResult second = await service.LoadAsync("GameScene");
        completion.SetResult(true);
        SceneTransitionResult firstResult = await first;

        Assert(second == SceneTransitionResult.AlreadyLoading, "Concurrent transition should be rejected.");
        Assert(firstResult == SceneTransitionResult.Succeeded, "Original transition should complete.");
        Assert(loader.LoadCount == 1, "Only one scene load should start.");
    }

    public async Task LoadAsync_RecoversAfterFailure()
    {
        FakeSceneLoader loader = new(canLoad: true, exception: new InvalidOperationException("test"));
        SceneFlowService service = new(loader);

        SceneTransitionResult result = await service.LoadAsync("LobbyScene");

        Assert(result == SceneTransitionResult.Failed, "Loader exception should become a failed result.");
        Assert(!service.IsLoading, "Loading state should reset after failure.");
    }

    private sealed class FakeSceneLoader : ISceneLoader
    {
        private readonly bool canLoad;
        private readonly Task pendingTask;
        private readonly Exception exception;

        public FakeSceneLoader(bool canLoad, Task pendingTask = null, Exception exception = null)
        {
            this.canLoad = canLoad;
            this.pendingTask = pendingTask;
            this.exception = exception;
        }

        public int LoadCount { get; private set; }

        public bool CanLoad(string sceneName) => canLoad;

        public async Task LoadAsync(string sceneName, Action<float> onProgress = null)
        {
            LoadCount++;
            if (exception != null)
                throw exception;
            if (pendingTask != null)
                await pendingTask;
            onProgress?.Invoke(1f);
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
