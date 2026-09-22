using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class UnitySceneLoader : ISceneLoader
{
    public bool CanLoad(string sceneName)
    {
        return !string.IsNullOrWhiteSpace(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName);
    }

    public async Task LoadAsync(string sceneName, Action<float> onProgress = null)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (operation == null)
            throw new InvalidOperationException($"Failed to start loading scene: {sceneName}");

        while (!operation.isDone)
        {
            onProgress?.Invoke(Mathf.Clamp01(operation.progress / 0.9f));
            await Task.Yield();
        }

        onProgress?.Invoke(1f);
    }
}
