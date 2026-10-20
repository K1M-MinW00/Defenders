using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class SceneFlowServiceTests
{
    [Test]
    public async Task LoadAsync_CompletesValidScene()
    {
        FakeSceneLoader loader = new(canLoad: true);
        SceneFlowService service = new(loader);

        SceneTransitionResult result = await service.LoadAsync("LobbyScene");

        Assert(result == SceneTransitionResult.Succeeded, "Valid scene should load successfully.");
        Assert(loader.LoadCount == 1, "Scene loader should run once.");
        Assert(!service.IsLoading, "Loading state should reset after success.");
    }

    [Test]
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

    [Test]
    public async Task LoadAsync_RecoversAfterFailure()
    {
        FakeSceneLoader loader = new(canLoad: true, exception: new InvalidOperationException("test"));
        SceneFlowService service = new(loader);
        LogAssert.Expect(LogType.Error, new Regex("\\[SceneFlowService\\] Failed to load 'LobbyScene'"));

        SceneTransitionResult result = await service.LoadAsync("LobbyScene");

        Assert(result == SceneTransitionResult.Failed, "Loader exception should become a failed result.");
        Assert(!service.IsLoading, "Loading state should reset after failure.");
    }

    [Test]
    public async Task LoadAsync_UsesLoadingSceneWhenConfigured()
    {
        FakeSceneLoader loader = new(canLoad: true);
        SceneFlowService service = new(loader, "LoadingScene");

        Task<SceneTransitionResult> transition = service.LoadAsync("GameScene");
        LoadingScreenState.MarkReady();
        SceneTransitionResult result = await transition;

        Assert(result == SceneTransitionResult.Succeeded, "Transition should complete.");
        Assert(loader.LoadCount == 2, "Loading scene and destination scene should both load.");
        Assert(loader.LoadedScenes[0] == "LoadingScene", "Loading scene should load first.");
        Assert(loader.LoadedScenes[1] == "GameScene", "Destination scene should load second.");
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
        public System.Collections.Generic.List<string> LoadedScenes { get; } = new();

        public bool CanLoad(string sceneName) => canLoad;

        public async Task LoadAsync(string sceneName, Action<float> onProgress = null)
        {
            LoadCount++;
            LoadedScenes.Add(sceneName);
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
