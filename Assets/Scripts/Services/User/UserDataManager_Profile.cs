using System.Threading.Tasks;

public partial class UserDataManager
{
    public async Task UpdateNicknameAsync(string nickname)
    {
        if (ProfileUpdateUseCase == null)
            return;

        if (await ProfileUpdateUseCase.UpdateNicknameAsync(nickname))
            RaiseProfileUpdated();
    }

    public async Task UpdateProfileIconAsync(string iconId)
    {
        if (ProfileUpdateUseCase == null)
            return;

        if (await ProfileUpdateUseCase.UpdateIconAsync(iconId))
            RaiseProfileUpdated();
    }
}
