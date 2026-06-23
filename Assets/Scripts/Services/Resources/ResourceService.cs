public class ResourceService
{
    private UserResourceData Resource => UserDataManager.Instance.UserData.Resource;

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        Resource.Gold += amount;

        UserDataManager.Instance.MarkDirty();
        UserDataManager.Instance.RaiseResourceUpdated();
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0)
            return false;

        if (Resource.Gold < amount)
            return false;

        Resource.Gold -= amount;

        UserDataManager.Instance.MarkDirty();
        UserDataManager.Instance.RaiseResourceUpdated();

        return true;
    }

    public void AddGem(int amount)
    {
        if (amount <= 0)
            return;

        Resource.Gem += amount;

        UserDataManager.Instance.MarkDirty();
        UserDataManager.Instance.RaiseResourceUpdated();
    }

    public bool SpendGem(int amount)
    {
        if (amount <= 0)
            return false;

        if (Resource.Gem < amount)
            return false;

        Resource.Gem -= amount;

        UserDataManager.Instance.MarkDirty();
        UserDataManager.Instance.RaiseResourceUpdated();

        return true;
    }

    public void AddFuel(int amount)
    {
        if (amount <= 0)
            return;

        StaminaService.AddFuel(Resource, amount);

        UserDataManager.Instance.MarkDirty();
        UserDataManager.Instance.RaiseResourceUpdated();
    }

    public bool ConsumeFuel(int amount)
    {
        if (amount <= 0)
            return false;

        bool success = StaminaService.ConsumeFuel(Resource, amount);

        if (!success)
            return false;

        UserDataManager.Instance.MarkDirty();
        UserDataManager.Instance.RaiseResourceUpdated();

        return true;
    }
}