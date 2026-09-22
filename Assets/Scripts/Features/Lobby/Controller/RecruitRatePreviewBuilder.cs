using System.Collections.Generic;

public static class RecruitRatePreviewBuilder
{
    public static IReadOnlyList<RecruitRatePreviewRow> Build(GachaDataSO banner)
    {
        List<RecruitRatePreviewRow> rows = new();
        if (banner == null)
            return rows;

        AddRarityRows(rows, banner, Rarity.Legend, "전설", banner.LegendRate);
        AddRarityRows(rows, banner, Rarity.Rare, "희귀", banner.RareRate);
        AddRarityRows(rows, banner, Rarity.Normal, "일반", banner.NormalRate);
        return rows;
    }

    private static void AddRarityRows(
        ICollection<RecruitRatePreviewRow> rows,
        GachaDataSO banner,
        Rarity rarity,
        string rarityLabel,
        float rarityRate)
    {
        IReadOnlyList<UnitDataSO> pool = banner.GetPool(rarity);
        UnitDataSO pickup = banner.RecruitType == RecruitType.Special ? banner.PickupUnit : null;

        if (pickup == null || pickup.rarity != rarity)
        {
            rows.Add(new RecruitRatePreviewRow($"{rarityLabel} 유닛", rarityRate, pool));
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
            rows.Add(new RecruitRatePreviewRow($"{pickup.displayName} (픽업)", rarityRate, new[] { pickup }));
            return;
        }

        float pickupRate = rarityRate * banner.PickupRateWithinRarity / 100f;
        rows.Add(new RecruitRatePreviewRow($"{pickup.displayName} (픽업)", pickupRate, new[] { pickup }));
        rows.Add(new RecruitRatePreviewRow($"{rarityLabel} 유닛", rarityRate - pickupRate, otherUnits));
    }
}
