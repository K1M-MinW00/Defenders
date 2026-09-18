using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Firestore;

public static class RewardGrantCalculator
{
    public static RewardGrantResult Calculate(
        UserDataRoot source,
        IReadOnlyCollection<RewardData> rewards)
    {
        if (source?.Resource == null || source.Inventory == null || source.Roster == null)
            return RewardGrantResult.Fail(RewardGrantFailure.InvalidData);

        if (rewards == null || rewards.Count == 0 || rewards.Any(reward => reward == null || reward.Amount <= 0))
            return RewardGrantResult.Fail(RewardGrantFailure.InvalidReward);

        UserResourceData resources = UserDataCloner.Copy(source.Resource);
        UserInventoryData inventory = UserDataCloner.Copy(source.Inventory);
        UserRosterData roster = UserDataCloner.Copy(source.Roster);

        try
        {
            foreach (RewardData reward in rewards)
            {
                if (!ApplyReward(reward, resources, inventory, roster))
                    return RewardGrantResult.Fail(RewardGrantFailure.InvalidReward);
            }
        }
        catch (OverflowException)
        {
            return RewardGrantResult.Fail(RewardGrantFailure.Overflow);
        }

        return RewardGrantResult.Success(resources, inventory, roster);
    }

    private static bool ApplyReward(
        RewardData reward,
        UserResourceData resources,
        UserInventoryData inventory,
        UserRosterData roster)
    {
        switch (reward.Type)
        {
            case RewardType.Gold:
                resources.Gold = checked(resources.Gold + reward.Amount);
                return true;

            case RewardType.Gem:
                resources.Gem = checked(resources.Gem + reward.Amount);
                return true;

            case RewardType.Fuel:
                resources.Fuel = checked(resources.Fuel + reward.Amount);

                if (resources.Fuel >= resources.MaxFuel)
                    resources.LastFuelUpdateTime = Timestamp.GetCurrentTimestamp();

                return true;

            case RewardType.Item:
                return AddStackItem(inventory, reward.Id, reward.Amount);

            case RewardType.Equipment:
                return AddEquipment(inventory, reward.Id, reward.Amount);

            case RewardType.Unit:
                return AddUnit(resources, roster, reward.Id, reward.Amount);

            default:
                return false;
        }
    }

    private static bool AddStackItem(UserInventoryData inventory, string itemId, int amount)
    {
        ItemDataSO item = ItemDatabase.Get(itemId);

        if (item == null || !item.Stackable || item.Category == ItemCategory.Equipment)
            return false;

        List<InventoryStackItem> target = item.Category == ItemCategory.Material
            ? inventory.Materials
            : inventory.Consumables;
        InventoryStackItem owned = target.FirstOrDefault(stack => stack != null && stack.ItemId == itemId);

        if (owned == null)
        {
            target.Add(new InventoryStackItem { ItemId = itemId, Count = amount });
            return true;
        }

        owned.Count = checked(owned.Count + amount);
        return true;
    }

    private static bool AddEquipment(UserInventoryData inventory, string itemId, int amount)
    {
        ItemDataSO item = ItemDatabase.Get(itemId);

        if (item == null || item.Category != ItemCategory.Equipment)
            return false;

        for (int i = 0; i < amount; i++)
        {
            inventory.Equipments.Add(new EquipmentItemData
            {
                UniqueId = Guid.NewGuid().ToString(),
                ItemId = itemId,
                Level = 1,
            });
        }

        return true;
    }

    private static bool AddUnit(
        UserResourceData resources,
        UserRosterData roster,
        string unitId,
        int amount)
    {
        UnitDataSO unit = UnitDatabase.Get(unitId);

        if (unit == null)
            return false;

        for (int i = 0; i < amount; i++)
        {
            UserUnitData owned = roster.OwnedUnits
                .FirstOrDefault(candidate => candidate != null && candidate.UnitId == unitId);

            if (owned == null)
            {
                roster.OwnedUnits.Add(new UserUnitData { UnitId = unitId, Level = 1 });
            }
            else if (owned.LimitBreak + owned.DuplicateCount < UnitLimitBreakUseCase.MaxLimitBreak)
            {
                owned.DuplicateCount++;
            }
            else
            {
                resources.Gem = checked(resources.Gem + GetDuplicateReward(unit.rarity));
            }
        }

        return true;
    }

    private static int GetDuplicateReward(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Normal => 30,
            Rarity.Rare => 100,
            Rarity.Legend => 300,
            _ => 0,
        };
    }
}
