using System;
using System.Threading.Tasks;

public sealed class ProfileUpdateUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;
    private readonly int nicknameChangeGemCost;
    private bool isExecuting;

    public ProfileUpdateUseCase(
        IUserDataRepository repository,
        string userId,
        UserDataRoot userData,
        int nicknameChangeGemCost = 500)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentException("User ID is null or empty.", nameof(userId))
            : userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
        this.nicknameChangeGemCost = Math.Max(0, nicknameChangeGemCost);
    }

    public async Task<NicknameChangeResult> UpdateNicknameAsync(string nickname)
    {
        string normalizedNickname = NicknamePolicy.Normalize(nickname);
        if (!NicknamePolicy.IsValid(normalizedNickname))
            return NicknameChangeResult.Fail(NicknameChangeFailure.InvalidNickname);

        if (isExecuting)
            return NicknameChangeResult.Fail(NicknameChangeFailure.Busy);

        if (userData.Profile == null || userData.Resource == null)
            return NicknameChangeResult.Fail(NicknameChangeFailure.SaveFailed);

        if (userData.Profile.Nickname == normalizedNickname)
            return NicknameChangeResult.Fail(NicknameChangeFailure.NoChange);

        int gemCost = userData.Profile.HasUsedFreeNicknameChange ? nicknameChangeGemCost : 0;
        if (userData.Resource.Gem < gemCost)
            return NicknameChangeResult.Fail(NicknameChangeFailure.InsufficientGem, gemCost);

        UserProfileData nextProfile = UserDataCloner.Copy(userData.Profile);
        UserResourceData nextResources = UserDataCloner.Copy(userData.Resource);
        nextProfile.Nickname = normalizedNickname;
        nextProfile.HasUsedFreeNicknameChange = true;
        nextResources.Gem -= gemCost;

        isExecuting = true;
        try
        {
            await repository.SaveSectionsAsync(userId, new UserDataUpdate
            {
                Profile = nextProfile,
                Resources = nextResources,
            });
        }
        catch
        {
            return NicknameChangeResult.Fail(NicknameChangeFailure.SaveFailed, gemCost);
        }
        finally
        {
            isExecuting = false;
        }

        userData.Profile = nextProfile;
        userData.Resource = nextResources;
        return NicknameChangeResult.Success(gemCost);
    }

    public Task<bool> UpdateIconAsync(string iconId)
    {
        if (!ProfileIconResolver.IsSelectable(iconId, userData.Roster))
            return Task.FromResult(false);

        return UpdateAsync(profile => profile.IconId = iconId);
    }

    private async Task<bool> UpdateAsync(Action<UserProfileData> applyChange)
    {
        if (isExecuting || userData.Profile == null)
            return false;

        UserProfileData nextProfile = UserDataCloner.Copy(userData.Profile);
        applyChange(nextProfile);
        isExecuting = true;

        try
        {
            await repository.SaveProfileAsync(userId, nextProfile);
        }
        catch
        {
            return false;
        }
        finally
        {
            isExecuting = false;
        }

        userData.Profile = nextProfile;
        return true;
    }
}
