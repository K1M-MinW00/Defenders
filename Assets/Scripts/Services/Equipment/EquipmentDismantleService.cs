using System;
using System.Collections.Generic;
using System.Linq;

public enum EquipmentDismantleFailure { None, InvalidData, EquipmentNotFound, EquipmentEquipped, RewardFailed }

public readonly struct EquipmentDismantleResult
{
    public bool Succeeded => Failure == EquipmentDismantleFailure.None;
    public EquipmentDismantleFailure Failure { get; }
    public int RefundedExp { get; }
    public IReadOnlyDictionary<string, int> Rewards { get; }
    private EquipmentDismantleResult(EquipmentDismantleFailure failure, int refundedExp = 0, IReadOnlyDictionary<string, int> rewards = null)
    { Failure = failure; RefundedExp = refundedExp; Rewards = rewards; }
    public static EquipmentDismantleResult Success(int exp, IReadOnlyDictionary<string, int> rewards) => new(EquipmentDismantleFailure.None, exp, rewards);
    public static EquipmentDismantleResult Fail(EquipmentDismantleFailure failure) => new(failure);
}

public sealed class EquipmentDismantleService
{
    private readonly IItemCatalog catalog;
    private readonly EquipmentBalanceConfigSO config;
    public EquipmentDismantleService(IItemCatalog catalog, EquipmentBalanceConfigSO config) { this.catalog = catalog; this.config = config; }

    public EquipmentDismantleResult Dismantle(UserInventoryData inventory, UserRosterData roster, IReadOnlyCollection<string> uniqueIds)
    {
        if (inventory?.Equipments == null || roster == null || uniqueIds == null || uniqueIds.Count == 0 || catalog == null || config == null)
            return EquipmentDismantleResult.Fail(EquipmentDismantleFailure.InvalidData);
        string[] ids = uniqueIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).ToArray();
        if (ids.Length == 0) return EquipmentDismantleResult.Fail(EquipmentDismantleFailure.InvalidData);

        EquipmentInventoryIndex index = new(inventory, roster);
        long total = 0;
        decimal refundRate = decimal.Round((decimal)config.EnhancementRefundRate, 4);
        foreach (string id in ids)
        {
            if (!index.TryGet(id, out EquipmentItemData equipment)) return EquipmentDismantleResult.Fail(EquipmentDismantleFailure.EquipmentNotFound);
            if (index.IsEquipped(id)) return EquipmentDismantleResult.Fail(EquipmentDismantleFailure.EquipmentEquipped);
            EquipmentDataSO definition = catalog.Get(equipment.ItemId) as EquipmentDataSO;
            if (definition == null) return EquipmentDismantleResult.Fail(EquipmentDismantleFailure.EquipmentNotFound);
            total += config.GetBaseDismantleExp(definition.Rarity) +
                (long)decimal.Floor(equipment.TotalInvestedExp * refundRate);
            if (total > int.MaxValue) return EquipmentDismantleResult.Fail(EquipmentDismantleFailure.RewardFailed);
        }

        Dictionary<string, int> rewards = ConvertToStones((int)total);
        InventoryMutationService mutations = new(catalog);
        foreach (KeyValuePair<string, int> reward in rewards)
            if (reward.Value > 0 && !mutations.Add(inventory, reward.Key, reward.Value).Succeeded)
                return EquipmentDismantleResult.Fail(EquipmentDismantleFailure.RewardFailed);

        HashSet<string> removed = new(ids, StringComparer.Ordinal);
        inventory.Equipments.RemoveAll(item => item != null && removed.Contains(item.UniqueId));
        return EquipmentDismantleResult.Success((int)total, rewards);
    }

    private Dictionary<string, int> ConvertToStones(int exp)
    {
        Dictionary<string, int> result = new(StringComparer.Ordinal);
        Add(config.LegendStoneItemId, 250); Add(config.EpicStoneItemId, 50); Add(config.RareStoneItemId, 10);
        return result;
        void Add(string id, int value) { int count = exp / value; exp %= value; if (count > 0) result[id] = count; }
    }
}
