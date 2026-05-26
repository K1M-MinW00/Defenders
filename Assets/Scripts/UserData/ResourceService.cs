using System.Threading.Tasks;

public class ResourceService
{
    private UserResourceData Resource => UserDataManager.Instance.UserData.Resource;

    public async Task AddGoldAsync(int amount)
    {
        if (amount <= 0)
            return;

        Resource.Gold += amount;

        UserDataManager.Instance.MarkDirty();
        UserDataManager.Instance.NotifyResourceUpdated();

        await UserDataManager.Instance.SaveAsync();
    }

    public async Task<bool> SpendGoldAsync(int amount)
    {
        if (Resource.Gold < amount)
            return false;

        Resource.Gold -= amount;

        UserDataManager.Instance.MarkDirty();
        UserDataManager.Instance.NotifyResourceUpdated();

        await UserDataManager.Instance.SaveAsync();

        return true;
    }

    public async Task AddGemAsync(int amount)
    {
        Resource.Gem += amount;

        UserDataManager.Instance.MarkDirty();
        UserDataManager.Instance.NotifyResourceUpdated();

        await UserDataManager.Instance.SaveAsync();
    }

    public async Task<bool> ConsumeFuelAsync(int amount)
    {
        bool success = StaminaService.ConsumeFuel(Resource, amount);

        if (!success)
            return false;

        UserDataManager.Instance.MarkDirty();
        UserDataManager.Instance.NotifyResourceUpdated();

        await UserDataManager.Instance.SaveAsync();

        return true;
    }
}