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
            Gacha = CreateDefaultGacha(),
        };
    }

    public static UserGachaData CreateDefaultGacha()
    {
        return new UserGachaData
        {
            NormalPity = 0,
            SpecialPity = 0,
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
        NewUserConfigSO config = GameConfig.NewUserConfig;

        return new UserResourceData
        {
            Gold = config.StartGold,
            Gem = config.StartGem,
            Fuel = config.StartFuel,
            MaxFuel = config.MaxFuel,
            LastFuelUpdateTime = GetNow()
        };
    }

    public static UserRosterData CreateDefaultRoster()
    {
        UserRosterData roster = new();

        foreach (string unitId in GameConfig.NewUserConfig.DefaultOwnedUnitIds)
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