using System.Collections.Generic;

public static class RecruitRatePreviewBuilder
{
    public static IReadOnlyList<RecruitRatePreviewRow> Build(GachaDataSO banner)
    {
        List<RecruitRatePreviewRow> rows = new();
        if (banner == null)
            return rows;

        AddRarityRows(rows, banner, Rarity.Legend, "recruit.rate.legend_units", banner.LegendRate);
        if (banner.EpicRate > 0f)
            AddRarityRows(rows, banner, Rarity.Epic, "recruit.rate.epic_units", banner.EpicRate);
        AddRarityRows(rows, banner, Rarity.Rare, "recruit.rate.rare_units", banner.RareRate);
        return rows;
    }

    private static void AddRarityRows(
        ICollection<RecruitRatePreviewRow> rows,
        GachaDataSO banner,
        Rarity rarity,
        string rarityLabelKey,
        float rarityRate)
    {
        IReadOnlyList<UnitDataSO> pool = banner.GetPool(rarity);
        UnitDataSO pickup = banner.RecruitType == RecruitType.Special ? banner.PickupUnit : null;
        string rarityLabel = Localize(rarityLabelKey);

        if (pickup == null || pickup.rarity != rarity)
        {
            rows.Add(new RecruitRatePreviewRow(rarityLabel, rarityRate, pool));
            return;
        }

        List<UnitDataSO> otherUnits = new();
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null && pool[i].unitId != pickup.unitId)
                otherUnits.Add(pool[i]);
        }

        if (otherUnits.Count == 0)
        {
            rows.Add(new RecruitRatePreviewRow(Localize("recruit.rate.pickup", pickup.displayName), rarityRate, new[] { pickup }));
            return;
        }

        float pickupRate = rarityRate * banner.PickupRateWithinRarity / 100f;
        rows.Add(new RecruitRatePreviewRow(Localize("recruit.rate.pickup", pickup.displayName), pickupRate, new[] { pickup }));
        rows.Add(new RecruitRatePreviewRow(rarityLabel, rarityRate - pickupRate, otherUnits));
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Lobby, key, arguments);
}
