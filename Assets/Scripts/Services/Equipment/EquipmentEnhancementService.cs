using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEquipmentRandom
{
    int Range(int minInclusive, int maxExclusive);
    float Range(float minInclusive, float maxInclusive);
}

public sealed class UnityEquipmentRandom : IEquipmentRandom
{
    public int Range(int minInclusive, int maxExclusive) => UnityEngine.Random.Range(minInclusive, maxExclusive);
    public float Range(float minInclusive, float maxInclusive) => UnityEngine.Random.Range(minInclusive, maxInclusive);
}

public enum EquipmentEnhancementFailure
{
    None, InvalidData, EquipmentNotFound, InvalidMaterials, InsufficientMaterials, MaxLevel
}

public readonly struct EquipmentEnhancementResult
{
    public bool Succeeded => Failure == EquipmentEnhancementFailure.None;
    public EquipmentEnhancementFailure Failure { get; }
    public int Level { get; }
    public int Exp { get; }
    public bool BonusGranted { get; }

    private EquipmentEnhancementResult(EquipmentEnhancementFailure failure, int level = 0, int exp = 0, bool bonusGranted = false)
    { Failure = failure; Level = level; Exp = exp; BonusGranted = bonusGranted; }
    public static EquipmentEnhancementResult Success(int level, int exp, bool bonusGranted) => new(EquipmentEnhancementFailure.None, level, exp, bonusGranted);
    public static EquipmentEnhancementResult Fail(EquipmentEnhancementFailure failure) => new(failure);
}

public sealed class EquipmentEnhancementService
{
    private readonly IItemCatalog catalog;
    private readonly EquipmentBalanceConfigSO config;
    private readonly IEquipmentRandom random;

    public EquipmentEnhancementService(IItemCatalog catalog, EquipmentBalanceConfigSO config, IEquipmentRandom random)
    { this.catalog = catalog; this.config = config; this.random = random; }

    public EquipmentEnhancementResult Enhance(UserInventoryData inventory, string uniqueId, IReadOnlyDictionary<string, int> materials)
    {
        if (inventory?.Equipments == null || catalog == null || config == null || random == null)
            return EquipmentEnhancementResult.Fail(EquipmentEnhancementFailure.InvalidData);

        EquipmentItemData equipment = inventory.Equipments.Find(item => item?.UniqueId == uniqueId);
        EquipmentDataSO definition = equipment == null ? null : catalog.Get(equipment.ItemId) as EquipmentDataSO;
        if (definition == null) return EquipmentEnhancementResult.Fail(EquipmentEnhancementFailure.EquipmentNotFound);
        if (equipment.Level >= EquipmentBalanceConfigSO.MaxLevel) return EquipmentEnhancementResult.Fail(EquipmentEnhancementFailure.MaxLevel);
        if (materials == null || materials.Count == 0) return EquipmentEnhancementResult.Fail(EquipmentEnhancementFailure.InvalidMaterials);

        int addedExp = 0;
        foreach (KeyValuePair<string, int> pair in materials)
        {
            MaterialDataSO material = catalog.Get(pair.Key) as MaterialDataSO;
            if (pair.Value <= 0 || material == null || material.MaterialType != MaterialType.Equipment || material.Value <= 0)
                return EquipmentEnhancementResult.Fail(EquipmentEnhancementFailure.InvalidMaterials);
            try { addedExp = checked(addedExp + checked(material.Value * pair.Value)); }
            catch (OverflowException) { return EquipmentEnhancementResult.Fail(EquipmentEnhancementFailure.InvalidMaterials); }
        }

        int required = config.GetRequiredExp(definition.Rarity, equipment.Level);
        int needed = Mathf.Max(0, required - equipment.EnhancementExp);
        if (required <= 0 || addedExp < needed)
            return EquipmentEnhancementResult.Fail(EquipmentEnhancementFailure.InsufficientMaterials);

        InventoryMutationService mutations = new(catalog);
        InventoryMutationResult consumed = mutations.ConsumeBatch(inventory, materials);
        if (!consumed.Succeeded)
            return EquipmentEnhancementResult.Fail(consumed.Failure == InventoryMutationFailure.InsufficientItems
                ? EquipmentEnhancementFailure.InsufficientMaterials : EquipmentEnhancementFailure.InvalidMaterials);

        // One request can advance exactly one level. Refund every representable 10 EXP
        // and retain the sub-10 remainder as progress so no enhancement value is lost.
        int excess = addedExp - needed;
        int retainedExp = excess % 10;
        int refundableExp = excess - retainedExp;
        equipment.TotalInvestedExp = checked(equipment.TotalInvestedExp + needed + retainedExp);
        equipment.EnhancementExp = retainedExp;
        equipment.Level++;
        if (refundableExp > 0)
        {
            foreach (KeyValuePair<string, int> reward in BuildStoneRefund(refundableExp))
            {
                InventoryMutationResult added = mutations.Add(inventory, reward.Key, reward.Value);
                if (!added.Succeeded)
                    return EquipmentEnhancementResult.Fail(EquipmentEnhancementFailure.InvalidData);
            }
        }

        bool bonusGranted = false;
        if (equipment.Level >= EquipmentBalanceConfigSO.MaxLevel)
        {
            equipment.EnhancementExp = 0;
            if (equipment.BonusOption == null)
            {
                equipment.BonusOption = RollBonus(definition.EquipmentType);
                bonusGranted = true;
            }
        }
        return EquipmentEnhancementResult.Success(equipment.Level, equipment.EnhancementExp, bonusGranted);
    }

    private Dictionary<string, int> BuildStoneRefund(int exp)
    {
        Dictionary<string, int> result = new();
        Add(config.LegendStoneItemId, 250);
        Add(config.EpicStoneItemId, 50);
        Add(config.RareStoneItemId, 10);
        return result;

        void Add(string id, int value)
        {
            int count = exp / value;
            if (count <= 0) return;
            result[id] = count;
            exp -= count * value;
        }
    }

    private EquipmentBonusOptionData RollBonus(EquipmentType type)
    {
        EquipmentStatType[] choices = type is EquipmentType.Helmet or EquipmentType.Armor
            ? new[] { EquipmentStatType.DamageReduction, EquipmentStatType.MaxHp }
            : new[] { EquipmentStatType.Attack, EquipmentStatType.CritChance };
        EquipmentStatType stat = choices[random.Range(0, choices.Length)];
        Vector2 range = config.GetBonusRange(stat);
        return new EquipmentBonusOptionData { StatType = stat, ModifierType = EquipmentModifierType.Percent, Value = random.Range(range.x, range.y) };
    }
}
