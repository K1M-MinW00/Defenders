using System;
using System.Threading.Tasks;

public interface ISceneLoader
{
    bool CanLoad(string sceneName);
    Task LoadAsync(string sceneName, Action<float> onProgress = null);
}
