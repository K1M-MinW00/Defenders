using System.Threading.Tasks;

public partial class UserDataManager
{
    public async Task<NicknameChangeResult> UpdateNicknameAsync(string nickname)
    {
        if (ProfileUpdateUseCase == null)
            return NicknameChangeResult.Fail(NicknameChangeFailure.SaveFailed);

        NicknameChangeResult result = await ProfileUpdateUseCase.UpdateNicknameAsync(nickname);
        if (result.Succeeded)
        {
            RaiseProfileUpdated();
            if (result.GemCost > 0)
                RaiseResourceUpdated();
        }

        return result;
    }

    public async Task<bool> UpdateProfileIconAsync(string iconId)
    {
        if (ProfileUpdateUseCase == null)
            return false;

        if (await ProfileUpdateUseCase.UpdateIconAsync(iconId))
        {
            RaiseProfileUpdated();
            return true;
        }

        return false;
    }
}
