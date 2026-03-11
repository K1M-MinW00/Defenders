public static class UserDataFactory
{
    public static UserDataRoot CreateDefault(string userId)
    {
        return new UserDataRoot
        {
            Profile = new UserProfileData
            {
                UserId = userId,
                Level = 1,
                Exp = 0
            },
            Roster = CreateDefaultRoster(),
            Progress = new()
        };
    }

    public static UserRosterData CreateDefaultRoster()
    {
        var roster = new UserRosterData();
        
        foreach(var unitCode in UnitMasterDataManager.Instance.DefaultOwnedUnits)
            roster.OwnedUnits.Add(new UserUnitData(unitCode));

        foreach (var unitCode in UnitMasterDataManager.Instance.StarterUnits)
            roster.BattleSquadUnitCodes.Add((int)unitCode);

        return roster;
    }
}
