using System.Collections.Generic;
using UnityEngine;

public enum StageRelicId
{
    WarlordsSeal,
    HawkeyeCrystal,
    BerserkersSpring,
    FrostShackles,
    ReinforcementFlare,
    EliteSummons,
    AbundantVein,
    DiceOfFate,
}

public sealed class StageRelicDefinition
{
    public StageRelicId Id { get; }
    public string Name => Localize("name");
    public string ShortDescription => Localize("short");
    public string Description => Localize("description");
    public string Symbol => Localize("symbol");
    public Sprite Icon
    {
        get
        {
            if (icon == null && !string.IsNullOrEmpty(iconResourcePath))
                icon = Resources.Load<Sprite>(iconResourcePath);

            return icon;
        }
    }

    private readonly string iconResourcePath;
    private Sprite icon;

    public StageRelicDefinition(
        StageRelicId id,
        string iconResourcePath)
    {
        Id = id;
        this.iconResourcePath = iconResourcePath;
    }

    private string Localize(string field) =>
        LocalizationService.Get(LocalizationTables.Relics, $"{Id}.{field}");
}

public static class StageRelicCatalog
{
    public static readonly IReadOnlyList<StageRelicDefinition> All = new[]
    {
        new StageRelicDefinition(StageRelicId.WarlordsSeal, "UI/Relics/relic_ally_attack"),
        new StageRelicDefinition(StageRelicId.HawkeyeCrystal, "UI/Relics/relic_ally_critical"),
        new StageRelicDefinition(StageRelicId.BerserkersSpring, "UI/Relics/relic_ally_attack_speed"),
        new StageRelicDefinition(StageRelicId.FrostShackles, "UI/Relics/relic_enemy_move_slow"),
        new StageRelicDefinition(StageRelicId.ReinforcementFlare, "UI/Relics/relic_random_one_star_units"),
        new StageRelicDefinition(StageRelicId.EliteSummons, "UI/Relics/relic_random_two_star_unit"),
        new StageRelicDefinition(StageRelicId.AbundantVein, "UI/Relics/relic_random_minerals"),
        new StageRelicDefinition(StageRelicId.DiceOfFate, "UI/Relics/relic_free_rerolls"),
    };
}
