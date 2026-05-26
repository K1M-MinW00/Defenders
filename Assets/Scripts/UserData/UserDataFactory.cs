using System;

public static class UserDataFactory
{
    public static UserDataRoot CreateDefault(string userId)
    {
        return new UserDataRoot
        {
            Profile = CreateDefaultProfile(userId),
            Resource = CreateDefaultResources(),
            Roster = CreateDefaultRoster(),
            Progress = CreateDefaultProgress(),
            Inventory = CreateDefaultInventory(),
        };
    }

    public static UserProfileData CreateDefaultProfile(string userId)
    {
        return new UserProfileData
        {
            UserId = userId,
            Nickname = $"User_{userId}",
            Level = 1,
            Exp = 0
        };
    }

    public static UserResourceData CreateDefaultResources()
    {
        return new UserResourceData
        {
            Gold = 0,
            Gem = 0,
            Fuel = 100,
            MaxFuel = 100,
            LastFuelUpdateTime = GetNow()
        };
    }

    public static UserRosterData CreateDefaultRoster()
    {
        UserRosterData roster = new();

        UnitMasterDataManager master = UnitMasterDataManager.Instance;

        foreach (string unitId in master.DefaultOwnedUnitIds)
        {
            roster.OwnedUnits.Add(new UserUnitData { UnitId = unitId, Level = 1,});
            roster.SelectedUnitIds.Add(unitId);
        }

        return roster;
    }

    public static UserProgressData CreateDefaultProgress()
    {
        return new UserProgressData
        {
            CurrentSector = 1,
            CurrentStage = 1,
            BestWaveCleared = 0
        };
    }
    public static UserInventoryData CreateDefaultInventory()
    {
        return new UserInventoryData();
    }

    private static long GetNow() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}