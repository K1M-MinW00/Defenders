using UnityEngine;

public static class GameConfig
{
    public static NewUserConfigSO NewUserConfig { get; private set; }
    public static UserLevelProgressionSO UserLevelProgression { get; private set; }
    public static GachaEconomyConfigSO GachaEconomy { get; private set; }
    public static IGameIconProvider Icons { get; private set; }
    public static IUnitCatalog Units { get; private set; }
    public static IItemCatalog Items { get; private set; }

    public static void Initialize()
    {
        if (NewUserConfig != null && UserLevelProgression != null && GachaEconomy != null &&
            Icons != null && Units != null && Items != null)
            return;

        NewUserConfig = Resources.Load<NewUserConfigSO>("Configs/NewUserConfig");
        UserLevelProgression = Resources.Load<UserLevelProgressionSO>("Configs/UserLevelProgression");
        GachaEconomy = Resources.Load<GachaEconomyConfigSO>("Configs/GachaEconomyConfig");

        GameIconSetSO iconSet = Resources.Load<GameIconSetSO>("Database/GameIconSet");
        Icons = new GameIconProvider(iconSet);
        Units = new UnitCatalog(Resources.LoadAll<UnitDataSO>("UnitData"));
        Items = new ItemCatalog(Resources.LoadAll<ItemDataSO>("Items"));
    }
}
