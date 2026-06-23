using UnityEngine;

public static class GameConfig
{
    public static NewUserConfigSO NewUserConfig { get; private set; }

    public static void Initialize()
    {
        if (NewUserConfig != null)
            return;

        NewUserConfig = Resources.Load<NewUserConfigSO>("Configs/NewUserConfig");
    }
}