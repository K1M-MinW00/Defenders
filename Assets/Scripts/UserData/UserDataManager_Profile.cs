using System.Threading.Tasks;

public partial class UserDataManager
{
    public async Task UpdateNicknameAsync(string nickname)
    {
        if (UserData == null)
            return;

        UserData.Profile.Nickname = nickname;

        await SaveUserDataAsync();
    }

    public async Task UpdateProfileIconAsync(string iconId)
    {
        if (UserData == null)
            return;

        UserData.Profile.IconId = iconId;

        await SaveUserDataAsync();

        OnProfileUpdated?.Invoke();
    }
}