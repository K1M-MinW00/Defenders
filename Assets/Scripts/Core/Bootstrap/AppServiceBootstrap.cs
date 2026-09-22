using UnityEngine;

public static class AppServiceBootstrap
{
    private const string RootName = "[App Services]";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateServices()
    {
        if (GameSettingsManager.Instance != null &&
            AuthService.Instance != null &&
            UserDataManager.Instance != null &&
            AdManager.Instance != null)
        {
            return;
        }

        GameObject root = new(RootName);
        if (GameSettingsManager.Instance == null)
            root.AddComponent<GameSettingsManager>();
        if (AuthService.Instance == null)
            root.AddComponent<AuthService>();
        if (UserDataManager.Instance == null)
            root.AddComponent<UserDataManager>();
        if (AdManager.Instance == null)
            root.AddComponent<AdManager>();
    }
}
