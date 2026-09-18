using System.Collections.Generic;

public static class UserDataNormalizer
{
    public static bool Normalize(UserDataRoot data, string userId)
    {
        if (data == null)
            return false;

        bool changed = false;

        if (data.Profile == null)
        {
            data.Profile = UserDataFactory.CreateDefaultProfile(userId);
            changed = true;
        }
        else if (string.IsNullOrEmpty(data.Profile.UserId))
        {
            data.Profile.UserId = userId;
            changed = true;
        }

        if (data.Resource == null)
        {
            data.Resource = UserDataFactory.CreateDefaultResources();
            changed = true;
        }

        if (data.Roster == null)
        {
            data.Roster = UserDataFactory.CreateDefaultRoster();
            changed = true;
        }

        if (data.Roster.OwnedUnits == null)
        {
            data.Roster.OwnedUnits = new List<UserUnitData>();
            changed = true;
        }

        if (data.Roster.SelectedUnitIds == null)
        {
            data.Roster.SelectedUnitIds = new List<string>();
            changed = true;
        }

        if (data.Progress == null)
        {
            data.Progress = UserDataFactory.CreateDefaultProgress();
            changed = true;
        }

        if (data.Inventory == null)
        {
            data.Inventory = UserDataFactory.CreateDefaultInventory();
            changed = true;
        }

        if (data.Inventory.Materials == null)
        {
            data.Inventory.Materials = new List<InventoryStackItem>();
            changed = true;
        }

        if (data.Inventory.Consumables == null)
        {
            data.Inventory.Consumables = new List<InventoryStackItem>();
            changed = true;
        }

        if (data.Inventory.Equipments == null)
        {
            data.Inventory.Equipments = new List<EquipmentItemData>();
            changed = true;
        }

        if (data.Gacha == null)
        {
            data.Gacha = UserDataFactory.CreateDefaultGacha();
            changed = true;
        }

        if (data.Ad == null)
        {
            data.Ad = UserDataFactory.CreateDefaultAd();
            changed = true;
        }

        return changed;
    }
}
