using UnityEngine;

public static class GameConfig
{
    public static NewUserConfigSO NewUserConfig { get; private set; }
    public static UserLevelProgressionSO UserLevelProgression { get; private set; }
    public static GachaEconomyConfigSO GachaEconomy { get; private set; }

    public static void Initialize()
    {
        if (NewUserConfig != null && UserLevelProgression != null && GachaEconomy != null)
            return;

        NewUserConfig = Resources.Load<NewUserConfigSO>("Configs/NewUserConfig");
        UserLevelProgression = Resources.Load<UserLevelProgressionSO>("Configs/UserLevelProgression");
        GachaEconomy = Resources.Load<GachaEconomyConfigSO>("Configs/GachaEconomyConfig");
    }
}
