using Firebase.Firestore;
using System;

public static class UserDataFactory
{
    public static UserDataRoot CreateDefault(string userId)
    {
        UserDataRoot data = new UserDataRoot
        {
            SchemaVersion = UserDataSchema.CurrentVersion,
            Profile = CreateDefaultProfile(userId),
            Resource = CreateDefaultResources(),
            Roster = CreateDefaultRoster(),
            Progress = CreateDefaultProgress(),
            Inventory = CreateDefaultInventory(),
            Gacha = CreateDefaultGacha(),
            Ad = CreateDefaultAd(),
            Shop = CreateDefaultShop(),
        };

        data.Profile.IconId = ProfileIconResolver.ResolveIconId(data.Profile.IconId, data.Roster);
        return data;
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
            Level = GameConfig.NewUserConfig.StartLevel,
            Exp = 0,
            IconId = ProfileIconResolver.ConfiguredDefaultIconId,
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
            LastFuelUpdateTime = Timestamp.GetCurrentTimestamp()
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

        roster.Power = FormationPowerCalculator.Calculate(roster, GameConfig.Units);

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


    public static UserAdData CreateDefaultAd()
    {
        return new UserAdData
        {
            FuelAdWatchCount = 0,
            GemAdWatchCount = 0,
        };
    }

    public static UserShopData CreateDefaultShop() => new();
}
