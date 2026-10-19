public static class EquipmentLocalizedText
{
    public static string StatName(EquipmentStatType type) => type switch
    {
        EquipmentStatType.MaxHp => Get("equipment.stat.max_hp"),
        EquipmentStatType.Attack => Get("equipment.stat.attack"),
        EquipmentStatType.CritChance => Get("equipment.stat.crit_chance"),
        _ => Get("equipment.stat.damage_reduction"),
    };

    public static string StatValue(EquipmentStatType type, float value) =>
        Get("equipment.stat.value", StatName(type), value);

    public static string BonusValue(
        EquipmentStatType type,
        EquipmentModifierType modifierType,
        float value) =>
        Get(
            "equipment.stat.bonus",
            StatName(type),
            value,
            modifierType == EquipmentModifierType.Percent ? "%" : string.Empty);

    private static string Get(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Items, key, arguments);
}
