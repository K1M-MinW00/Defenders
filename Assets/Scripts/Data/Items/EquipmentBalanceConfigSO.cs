using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentBalanceConfig", menuName = "Game/Equipment/Balance Config")]
public sealed class EquipmentBalanceConfigSO : ScriptableObject
{
    public const int MaxLevel = 10;

    [Header("Required EXP: Rare 0->1 through 9->10")]
    [SerializeField] private int[] rareRequiredExp = { 10, 15, 20, 30, 40, 55, 70, 90, 115, 150 };
    [SerializeField, Min(1f)] private float epicCostMultiplier = 2f;
    [SerializeField, Min(1f)] private float legendCostMultiplier = 4f;

    [Header("Stat Growth")]
    [SerializeField, Min(0f)] private float baseStatGrowthPerLevel = 0.1f;

    [Header("Dismantle")]
    [SerializeField, Range(0f, 1f)] private float enhancementRefundRate = 0.9f;
    [SerializeField, Min(0)] private int rareBaseDismantleExp = 20;
    [SerializeField, Min(0)] private int epicBaseDismantleExp = 100;
    [SerializeField, Min(0)] private int legendBaseDismantleExp = 500;

    [Header("Enhancement Stone IDs (low to high)")]
    [SerializeField] private string rareStoneItemId = "equipment_stone_rare";
    [SerializeField] private string epicStoneItemId = "equipment_stone_epic";
    [SerializeField] private string legendStoneItemId = "equipment_stone_legend";

    [Header("Level 10 Bonus (%)")]
    [SerializeField] private Vector2 damageReductionRange = new(3f, 8f);
    [SerializeField] private Vector2 maxHpRange = new(5f, 15f);
    [SerializeField] private Vector2 attackRange = new(5f, 15f);
    [SerializeField] private Vector2 critChanceRange = new(2f, 7f);

    public float BaseStatGrowthPerLevel => baseStatGrowthPerLevel;
    public float EnhancementRefundRate => enhancementRefundRate;
    public string RareStoneItemId => rareStoneItemId;
    public string EpicStoneItemId => epicStoneItemId;
    public string LegendStoneItemId => legendStoneItemId;

    public int GetRequiredExp(Rarity rarity, int level)
    {
        if (level < 0 || level >= MaxLevel || rareRequiredExp == null || level >= rareRequiredExp.Length)
            return 0;

        float multiplier = rarity switch
        {
            Rarity.Epic => epicCostMultiplier,
            Rarity.Legend => legendCostMultiplier,
            _ => 1f,
        };
        return Mathf.Max(1, Mathf.CeilToInt(rareRequiredExp[level] * multiplier));
    }

    public float GetBaseStatValue(EquipmentDataSO definition, int level) =>
        definition == null ? 0f : definition.BaseStatValue * (1f + Mathf.Clamp(level, 0, MaxLevel) * baseStatGrowthPerLevel);

    public int GetBaseDismantleExp(Rarity rarity) => rarity switch
    {
        Rarity.Legend => legendBaseDismantleExp,
        Rarity.Epic => epicBaseDismantleExp,
        _ => rareBaseDismantleExp,
    };

    public Vector2 GetBonusRange(EquipmentStatType statType) => statType switch
    {
        EquipmentStatType.DamageReduction => damageReductionRange,
        EquipmentStatType.MaxHp => maxHpRange,
        EquipmentStatType.Attack => attackRange,
        EquipmentStatType.CritChance => critChanceRange,
        _ => Vector2.zero,
    };

    public bool TryValidate(out string error) => TryValidate(null, out error);

    public bool TryValidate(IItemCatalog catalog, out string error)
    {
        if (rareRequiredExp == null || rareRequiredExp.Length != MaxLevel)
        {
            error = $"Rare required EXP must contain {MaxLevel} entries.";
            return false;
        }
        foreach (int value in rareRequiredExp)
        {
            if (value <= 0) { error = "Required EXP must be positive."; return false; }
        }
        if (string.IsNullOrWhiteSpace(rareStoneItemId) || string.IsNullOrWhiteSpace(epicStoneItemId) || string.IsNullOrWhiteSpace(legendStoneItemId))
        {
            error = "Enhancement stone IDs are missing.";
            return false;
        }
        if (catalog != null && (!IsValidStone(catalog, rareStoneItemId, 10) ||
            !IsValidStone(catalog, epicStoneItemId, 50) || !IsValidStone(catalog, legendStoneItemId, 250)))
        {
            error = "Enhancement stones must be Equipment materials with values 10, 50, and 250.";
            return false;
        }
        error = string.Empty;
        return true;
    }

    private static bool IsValidStone(IItemCatalog catalog, string id, int value) =>
        catalog.Get(id) is MaterialDataSO material && material.MaterialType == MaterialType.Equipment && material.Value == value;
}
