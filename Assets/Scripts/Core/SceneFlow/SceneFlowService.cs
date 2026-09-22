using System;
using System.Threading.Tasks;
using UnityEngine;

public sealed class SceneFlowService
{
    private static SceneFlowService shared = new(new UnitySceneLoader());
    private readonly ISceneLoader sceneLoader;

    public static SceneFlowService Shared => shared;
    public bool IsLoading { get; private set; }

    public SceneFlowService(ISceneLoader sceneLoader)
    {
        this.sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
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
            await sceneLoader.LoadAsync(sceneName, onProgress);
            return SceneTransitionResult.Succeeded;
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SceneFlowService] Failed to load '{sceneName}': {exception}");
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
        shared = new SceneFlowService(new UnitySceneLoader());
    }
}
