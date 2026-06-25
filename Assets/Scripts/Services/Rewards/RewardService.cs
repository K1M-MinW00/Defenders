using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardService
{
    public void GiveRewards(List<RewardData> rewards)
    {
        if (rewards == null)
            return;

        foreach (RewardData reward in rewards)
        {
            GiveReward(reward);
        }
    }

    public void GiveReward(RewardData reward)
    {
        if (reward == null)
            return;

        UserDataRoot userData = UserDataManager.Instance.UserData;

        switch (reward.Type)
        {
            case RewardType.Gold:
                UserDataManager.Instance.ResourceService.AddGold(reward.Amount);
                break;

            case RewardType.Gem:
                UserDataManager.Instance.ResourceService.AddGem(reward.Amount);
                break;

            case RewardType.Fuel:
                UserDataManager.Instance.ResourceService.AddFuel(reward.Amount);
                break;

            case RewardType.Item:
                GiveItem(reward.Id,reward.Amount);
                break;

            case RewardType.Unit:
                UserDataManager.Instance.RosterService.GiveUnit(UnitDatabase.Get(reward.Id));
                break;

            case RewardType.Equipment:
                GiveEquipment(reward.Id,reward.Amount);
                break;

            default:
                Debug.LogWarning($"Unhandled RewardType : {reward.Type}");
                break;
        }
    }

    private void GiveItem(string itemId, int amount)
    {
        ItemDataSO item = ItemDatabase.Get(itemId);

        if (item == null)
        {
            Debug.LogError($"Item not found : {itemId}");
            return;
        }

        InventoryService inventoryService = UserDataManager.Instance.InventoryService;
        Debug.Log($"{item.Category}, {itemId}, {amount}");
        inventoryService.AddStackItem(item.Category, itemId, amount);
    }

    private void GiveEquipment(string equipmentId, int amount)
    {
        Debug.Log($"Give Equipment : {equipmentId}");

        ItemDataSO equipment = ItemDatabase.Get(equipmentId);

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