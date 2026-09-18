using System.Collections.Generic;

public readonly struct UnitTrainingPreview
{
    public int Level { get; }
    public int Exp { get; }
    public int TotalExp { get; }
    public int GoldCost { get; }
    public bool CanAfford { get; }

    public UnitTrainingPreview(int level, int exp, int totalExp, int goldCost, bool canAfford)
    {
        Level = level;
        Exp = exp;
        TotalExp = totalExp;
        GoldCost = goldCost;
        CanAfford = canAfford;
    }
}

public static class UnitTrainingPreviewCalculator
{
    public static UnitTrainingPreview Calculate(
        UserUnitData unit,
        int maxLevel,
        IReadOnlyDictionary<string, int> selectedMaterials,
        int ownedGold)
    {
        if (unit == null)
            return default;

        long totalExp = 0;

        if (selectedMaterials != null)
        {
            foreach (KeyValuePair<string, int> pair in selectedMaterials)
            {
                if (pair.Value <= 0)
                    continue;

                MaterialDataSO material = ItemDatabase.Get(pair.Key) as MaterialDataSO;

                if (material == null || material.MaterialType != MaterialType.Training || material.Value <= 0)
                    continue;

                totalExp += (long)material.Value * pair.Value;

                if (totalExp >= int.MaxValue)
                {
                    totalExp = int.MaxValue;
                    break;
                }
            }
        }

        int total = (int)totalExp;
        int level = unit.Level;
        int exp = unit.Exp + total;

        while (level < maxLevel)
        {
            int requiredExp = UnitExpTable.GetRequiredExp(level);

            if (requiredExp <= 0 || exp < requiredExp)
                break;

            exp -= requiredExp;
            level++;
        }

        if (level >= maxLevel)
        {
            level = maxLevel;
            exp = 0;
        }

        return new UnitTrainingPreview(level, exp, total, total, ownedGold >= total);
    }
}
