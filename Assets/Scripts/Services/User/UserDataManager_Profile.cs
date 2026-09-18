using System.Threading.Tasks;

public partial class UserDataManager
{
    public async Task UpdateNicknameAsync(string nickname)
    {
        if (UserData == null)
            return;

        string previousNickname = UserData.Profile.Nickname;
        UserData.Profile.Nickname = nickname;

        bool success = await SaveProfileAsync(UserData.Profile);

        if (!success)
            UserData.Profile.Nickname = previousNickname;

        RaiseProfileUpdated();
    }

    public async Task UpdateProfileIconAsync(string iconId)
    {
        if (UserData == null)
            return;

        string previousIconId = UserData.Profile.IconId;
        UserData.Profile.IconId = iconId;

        bool success = await SaveProfileAsync(UserData.Profile);

        if (!success)
            UserData.Profile.IconId = previousIconId;
        
        RaiseProfileUpdated();
    }
}
