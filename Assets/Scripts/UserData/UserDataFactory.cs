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
            Roster = new(),
            Progress = new()
        };
    }
}
