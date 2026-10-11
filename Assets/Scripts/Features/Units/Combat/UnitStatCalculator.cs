public static class UnitStatCalculator
{
    public static UnitStats Calculate(UnitDataSO unitData, UserUnitData userUnit)
        => Calculate(unitData, userUnit, null, null, null);

    public static UnitStats Calculate(UnitDataSO unitData, UserUnitData userUnit, UserInventoryData inventory,
        IItemCatalog itemCatalog = null, EquipmentBalanceConfigSO equipmentConfig = null)
    {
        if (unitData == null || userUnit == null)
            return default;

        UnitStats stats = unitData.GetOriginStats(userUnit.Level);

        UnitStatModifier modifier = new();

        ApplyPromotion(modifier, userUnit.Promotion);

        ApplyLimitBreak(modifier, unitData, userUnit.LimitBreak);

        ApplyEquipment(modifier, userUnit, inventory, itemCatalog ?? GameConfig.Items,
            equipmentConfig ?? GameConfig.EquipmentBalance, ref stats);

        ApplyModifier(ref stats, modifier);

        stats.Attack = LabBonusProvider.ApplyPercent(stats.Attack, LabEffectType.AttackPercent);
        stats.MaxHp = LabBonusProvider.ApplyPercent(stats.MaxHp, LabEffectType.MaxHpPercent);
        stats.CritChance = UnityEngine.Mathf.Clamp01(
            stats.CritChance + LabBonusProvider.GetTotal(LabEffectType.CriticalChance) / 100f);
        stats.CritDamage = LabBonusProvider.ApplyPercent(stats.CritDamage, LabEffectType.CriticalDamagePercent);
        stats.AttackPerSec = LabBonusProvider.ApplyPercent(stats.AttackPerSec, LabEffectType.AttackSpeedPercent);
        stats.DetectRange = LabBonusProvider.ApplyPercent(stats.DetectRange, LabEffectType.DetectionRangePercent);
        stats.EnergyRecovery = LabBonusProvider.ApplyPercent(stats.EnergyRecovery, LabEffectType.EnergyRecoveryPercent);

        return stats;
    }

    private static void ApplyEquipment(UnitStatModifier modifier, UserUnitData unit, UserInventoryData inventory,
        IItemCatalog catalog, EquipmentBalanceConfigSO config, ref UnitStats stats)
    {
        if (unit?.Equipment == null || inventory?.Equipments == null || catalog == null || config == null) return;
        foreach (string id in unit.Equipment.EnumerateIds())
        {
            EquipmentItemData item = inventory.Equipments.Find(candidate => candidate?.UniqueId == id);
            EquipmentDataSO definition = item == null ? null : catalog.Get(item.ItemId) as EquipmentDataSO;
            if (definition == null) continue;
            float baseValue = config.GetBaseStatValue(definition, item.Level);
            if (definition.BaseStatType == EquipmentStatType.Attack) modifier.AttackFlat += baseValue;
            else if (definition.BaseStatType == EquipmentStatType.MaxHp) modifier.MaxHpFlat += baseValue;

            EquipmentBonusOptionData bonus = item.BonusOption;
            if (bonus == null) continue;
            bool percent = bonus.ModifierType == EquipmentModifierType.Percent;
            switch (bonus.StatType)
            {
                case EquipmentStatType.Attack: if (percent) modifier.AttackPercent += bonus.Value; else modifier.AttackFlat += bonus.Value; break;
                case EquipmentStatType.MaxHp: if (percent) modifier.MaxHpPercent += bonus.Value; else modifier.MaxHpFlat += bonus.Value; break;
                case EquipmentStatType.CritChance: stats.CritChance += percent ? bonus.Value / 100f : bonus.Value; break;
                case EquipmentStatType.DamageReduction: stats.DamageReduction += bonus.Value; break;
            }
        }
    }

    private static void ApplyPromotion(UnitStatModifier modifier, int promotion)
    {
        PromotionProgressionSO progression = PromotionProgressionDatabase.Get();

        if (progression == null)
            return;

        foreach (PromotionStatBonus bonus in progression.GetUnlockedStatBonuses(promotion))
            AddPercentModifier(modifier, bonus.statType, bonus.percentValue);
    }

    private static void ApplyLimitBreak(UnitStatModifier modifier,UnitDataSO unitData,int limitBreak)
    {
        if (limitBreak <= 0)
            return;

        int effectCount = UnityEngine.Mathf.Min(limitBreak, unitData.limitBreaks?.Count ?? 0);
        for (int i = 0; i < effectCount; i++)
        {
            LimitBreakData effect = unitData.limitBreaks[i];

            if (effect == null)
                continue;

            AddPercentModifier(modifier, effect.statType, effect.value);
        }
    }

    private static void AddPercentModifier(UnitStatModifier modifier,StatType statType,float value)
    {
        switch (statType)
        {
            case StatType.Attack:
                modifier.AttackPercent += value;
                break;

            case StatType.MaxHp:
                modifier.MaxHpPercent += value;
                break;

            case StatType.AttackPerSec:
                modifier.AttackPerSecPercent += value;
                break;

            case StatType.DetectRange:
                modifier.DetectRangePercent += value;
                break;

            case StatType.CritChance:
                modifier.CritChancePercent += value;
                break;

            case StatType.CritDamage:
                modifier.CritDamagePercent += value;
                break;

            case StatType.EnergyRecovery:
                modifier.EnergyRecoveryPercent += value;
                break;
        }
    }
    private static void ApplyModifier(ref UnitStats stats, UnitStatModifier modifier)
    {
        stats.Attack = (stats.Attack + modifier.AttackFlat) * (1f + modifier.AttackPercent / 100f);

        stats.MaxHp = (stats.MaxHp + modifier.MaxHpFlat) * (1f + modifier.MaxHpPercent / 100f);

        stats.AttackPerSec = (stats.AttackPerSec + modifier.AttackPerSecFlat) * (1f + modifier.AttackPerSecPercent / 100f);

        stats.DetectRange = (stats.DetectRange + modifier.DetectRangeFlat) * (1f + modifier.DetectRangePercent / 100f);

        stats.CritChance = (stats.CritChance + modifier.CritChanceFlat) * (1f + modifier.CritChancePercent / 100f);

        stats.CritDamage = (stats.CritDamage + modifier.CritDamageFlat) * (1f + modifier.CritDamagePercent / 100f);

        stats.EnergyRecovery = (stats.EnergyRecovery + modifier.EnergyRecoveryFlat) * (1f + modifier.EnergyRecoveryPercent / 100f);
    }
}
