using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RewardService
{
    public static void GiveRewards(List<RewardData> rewards)
    {
        if (rewards == null)
            return;

        foreach (RewardData reward in rewards)
        {
            GiveReward(reward);
        }
    }

    public static void GiveReward(RewardData reward)
    {
        if (reward == null)
            return;

        UserDataRoot userData = UserDataManager.Instance.UserData;

        switch (reward.Type)
        {
            case RewardType.Gold:
                GiveGold(reward.Amount);
                break;

            case RewardType.Gem:
                GiveGem(reward.Amount);
                break;

            case RewardType.Fuel:
                GiveFuel(reward.Amount);
                break;

            case RewardType.Item:
                GiveItem(reward.Id,reward.Amount);
                break;

            case RewardType.Unit:
                GiveUnit(reward.Id,reward.Amount);
                break;

            case RewardType.Equipment:
                GiveEquipment(reward.Id,reward.Amount);
                break;

            default:
                Debug.LogWarning($"Unhandled RewardType : {reward.Type}");
                break;
        }
    }

    private static void GiveGold(int amount)
    {
        UserDataManager.Instance.UserData.Resource.Gold += amount;
        UserDataManager.Instance.MarkDirty();
    }

    private static void GiveGem(int amount)
    {
        UserDataManager.Instance.UserData.Resource.Gem += amount;
        UserDataManager.Instance.MarkDirty();
    }

    private static void GiveFuel(int amount)
    {
        UserDataManager.Instance.UserData.Resource.Fuel += amount;
        UserDataManager.Instance.MarkDirty();
    }

    private static void GiveItem(string itemId, int amount)
    {
        ItemDataSO item = ItemDatabase.GetItem(itemId);

        if (item == null)
        {
            Debug.LogError($"Item not found : {itemId}");
            return;
        }

        InventoryService inventoryService = UserDataManager.Instance.InventoryService;
        Debug.Log($"{item.Category}, {itemId}, {amount}");
        inventoryService.AddStackItem(item.Category, itemId, amount);
    }

    private static void GiveUnit(string unitId, int amount)
    {
        Debug.Log($"Give Unit : {unitId} x{amount}");

        UnitDataSO unit = UnitDatabase.GetUnit(unitId);

        if (unit == null)
        {
            Debug.LogError($"Unit not found : {unitId}");

            return;
        }

        UserRosterData roster = UserDataManager.Instance.UserData.Roster;

        bool alreadyOwned = roster.OwnedUnits.Any(x => x.UnitId == unitId);

        if (alreadyOwned)
        {
            Debug.Log($"Already owned unit : {unitId}");
            return;
        }

        UserUnitData userUnit = new()
        {
            UnitId = unitId,
            Level = 1,
        };

        roster.OwnedUnits.Add(userUnit);

        UserDataManager.Instance.MarkDirty();
    }

    private static void GiveEquipment(string equipmentId, int amount)
    {
        Debug.Log($"Give Equipment : {equipmentId}");

        ItemDataSO equipment = ItemDatabase.GetItem(equipmentId);

        if (equipment == null)
        {
            Debug.LogError($"Equipment not found : {equipmentId}");
            return;
        }

        InventoryService inventoryService = UserDataManager.Instance.InventoryService;

        for (int i = 0; i < amount; i++)
        {
            inventoryService.AddEquipment(equipmentId);
        }
    }
}