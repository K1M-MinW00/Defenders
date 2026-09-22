using UnityEngine;

public static class GameConfig
{
    public static NewUserConfigSO NewUserConfig { get; private set; }
    public static UserLevelProgressionSO UserLevelProgression { get; private set; }
    public static GachaEconomyConfigSO GachaEconomy { get; private set; }
    public static IGameIconProvider Icons { get; private set; }
    public static IUnitCatalog Units { get; private set; }
    public static IItemCatalog Items { get; private set; }
    public static bool IsInitialized => NewUserConfig != null && UserLevelProgression != null &&
        GachaEconomy != null && Icons != null && Units != null && Items != null;

    public static void Initialize()
    {
        if (IsInitialized)
            return;

        NewUserConfigSO newUserConfig = LoadRequired<NewUserConfigSO>("Configs/NewUserConfig");
        UserLevelProgressionSO levelProgression = LoadRequired<UserLevelProgressionSO>("Configs/UserLevelProgression");
        GachaEconomyConfigSO gachaEconomy = LoadRequired<GachaEconomyConfigSO>("Configs/GachaEconomyConfig");
        GameIconSetSO iconSet = LoadRequired<GameIconSetSO>("Database/GameIconSet");
        IUnitCatalog units = new UnitCatalog(Resources.LoadAll<UnitDataSO>("UnitData"));
        IItemCatalog items = new ItemCatalog(Resources.LoadAll<ItemDataSO>("Items"));
        IGameIconProvider icons = new GameIconProvider(iconSet);

        Validate(newUserConfig.TryValidate(units, out string newUserError), "NewUserConfig", newUserError);
        Validate(levelProgression.TryValidate(out string levelError), "UserLevelProgression", levelError);
        Validate(gachaEconomy.TryValidate(out string economyError), "GachaEconomyConfig", economyError);

        NewUserConfig = newUserConfig;
        UserLevelProgression = levelProgression;
        GachaEconomy = gachaEconomy;
        Units = units;
        Items = items;
        Icons = icons;
    }

    private static T LoadRequired<T>(string path) where T : UnityEngine.Object
    {
        T asset = Resources.Load<T>(path);
        return asset != null
            ? asset
            : throw new System.InvalidOperationException($"Required game config is missing: Resources/{path}");
    }

    private static void Validate(bool isValid, string configName, string error)
    {
        if (!isValid)
            throw new System.InvalidOperationException($"Invalid {configName}: {error}");
    }
}
