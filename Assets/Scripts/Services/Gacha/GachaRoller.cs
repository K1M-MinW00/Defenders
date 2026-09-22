using System;
using System.Collections.Generic;

public sealed class GachaRoller
{
    private readonly IGachaRandom random;

    public GachaRoller(IGachaRandom random)
    {
        this.random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public Rarity RollRarity(GachaDataSO banner)
    {
        float roll = random.Range(0f, 100f);

        if (roll < banner.LegendRate)
            return Rarity.Legend;

        roll -= banner.LegendRate;
        return roll < banner.RareRate ? Rarity.Rare : Rarity.Normal;
    }

    public UnitDataSO RollUnit(GachaDataSO banner, Rarity rarity, UnitDataSO pickupUnit)
    {
        IReadOnlyList<UnitDataSO> pool = banner.GetPool(rarity);
        if (pool == null || pool.Count == 0)
            return null;

        if (pickupUnit == null || pickupUnit.rarity != rarity)
            return pool[random.Range(0, pool.Count)];

        if (pool.Count == 1 || random.Range(0f, 100f) < banner.PickupRateWithinRarity)
            return pickupUnit;

        int nonPickupCount = 0;
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].unitId != pickupUnit.unitId)
                nonPickupCount++;
        }

        if (nonPickupCount == 0)
            return pickupUnit;

        int selectedIndex = random.Range(0, nonPickupCount);
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].unitId == pickupUnit.unitId)
                continue;

            if (selectedIndex-- == 0)
                return pool[i];
        }

        return null;
    }
}
