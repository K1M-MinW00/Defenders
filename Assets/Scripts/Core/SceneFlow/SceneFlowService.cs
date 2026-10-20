using System;
using System.Threading.Tasks;
using UnityEngine;

public sealed class SceneFlowService
{
    private const string DefaultLoadingScene = "LoadingScene";
    private const int MinimumLoadingScreenMilliseconds = 650;

    private static SceneFlowService shared = new(new UnitySceneLoader(), DefaultLoadingScene);
    private readonly ISceneLoader sceneLoader;
    private readonly string loadingSceneName;

    public static SceneFlowService Shared => shared;
    public bool IsLoading { get; private set; }

    public SceneFlowService(ISceneLoader sceneLoader, string loadingSceneName = null)
    {
        this.sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
        this.loadingSceneName = loadingSceneName;
    }

    public async Task<SceneTransitionResult> LoadAsync(string sceneName, Action<float> onProgress = null)
    {
        if (IsLoading)
            return SceneTransitionResult.AlreadyLoading;
        if (!sceneLoader.CanLoad(sceneName))
            return SceneTransitionResult.InvalidScene;

        IsLoading = true;
        try
        {
            bool useLoadingScreen = !string.IsNullOrWhiteSpace(loadingSceneName) &&
                                    !string.Equals(sceneName, loadingSceneName, StringComparison.Ordinal) &&
                                    sceneLoader.CanLoad(loadingSceneName);

            if (useLoadingScreen)
            {
                LoadingScreenState.Begin(sceneName);
                await sceneLoader.LoadAsync(loadingSceneName);
                await LoadingScreenState.WaitUntilReadyAsync();
                await Task.Delay(MinimumLoadingScreenMilliseconds);
                await sceneLoader.LoadAsync(sceneName, progress =>
                {
                    LoadingScreenState.ReportProgress(progress);
                    onProgress?.Invoke(progress);
                });
            }
            else
            {
                await sceneLoader.LoadAsync(sceneName, onProgress);
            }
            return SceneTransitionResult.Succeeded;
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SceneFlowService] Failed to load '{sceneName}': {exception}");
            LoadingScreenState.ReportFailure("LOAD FAILED");
            return SceneTransitionResult.Failed;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetShared()
    {
        shared = new SceneFlowService(new UnitySceneLoader(), DefaultLoadingScene);
    }
}
