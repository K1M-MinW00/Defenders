using System;
using System.Threading.Tasks;

public sealed class ProfileUpdateUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;
    private bool isExecuting;

    public ProfileUpdateUseCase(
        IUserDataRepository repository,
        string userId,
        UserDataRoot userData)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentException("User ID is null or empty.", nameof(userId))
            : userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
    }

    public Task<bool> UpdateNicknameAsync(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            return Task.FromResult(false);

        return UpdateAsync(profile => profile.Nickname = nickname);
    }

    public Task<bool> UpdateIconAsync(string iconId)
    {
        if (string.IsNullOrWhiteSpace(iconId))
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
