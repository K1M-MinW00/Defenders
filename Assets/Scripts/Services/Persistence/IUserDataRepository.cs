using System.Threading.Tasks;

public interface IUserDataRepository
{
    Task<UserDataLoadResult> LoadAsync(string userId);
    Task CreateAsync(string userId, UserDataRoot data);
    Task SaveAllAsync(string userId, UserDataRoot data);
    Task SaveProfileAsync(string userId, UserProfileData profile);
    Task SaveResourcesAsync(string userId, UserResourceData resources);
    Task SaveProgressAsync(string userId, UserProgressData progress);
    Task SaveRosterAsync(string userId, UserRosterData roster);
    Task SaveInventoryAsync(string userId, UserInventoryData inventory);
    Task SaveGachaAsync(string userId, UserGachaData gacha);
    Task SaveAdAsync(string userId, UserAdData ad);
    Task SaveSectionsAsync(string userId, UserDataUpdate update);
}

public sealed class UserDataUpdate
{
    public UserProfileData Profile { get; set; }
    public UserResourceData Resources { get; set; }
    public UserProgressData Progress { get; set; }
    public UserRosterData Roster { get; set; }
    public UserInventoryData Inventory { get; set; }
    public UserGachaData Gacha { get; set; }
    public UserAdData Ad { get; set; }
}

public sealed class UserDataLoadResult
{
    public bool Exists { get; }
    public UserDataRoot Data { get; }

    private UserDataLoadResult(bool exists, UserDataRoot data)
    {
        Exists = exists;
        Data = data;
    }

    public static UserDataLoadResult Found(UserDataRoot data)
    {
        return new UserDataLoadResult(true, data);
    }

    public static UserDataLoadResult NotFound()
    {
        return new UserDataLoadResult(false, null);
    }
}
