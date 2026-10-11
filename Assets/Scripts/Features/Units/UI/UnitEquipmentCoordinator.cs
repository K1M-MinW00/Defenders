using System.Threading.Tasks;

/// <summary>
/// Executes unit-equipment mutations and prevents overlapping requests.
/// </summary>
public sealed class UnitEquipmentCoordinator
{
    public bool IsBusy { get; private set; }

    public async Task<EquipmentOperationFailure> EquipAsync(
        string unitId,
        string equipmentUniqueId)
    {
        if (IsBusy)
            return EquipmentOperationFailure.InvalidRequest;

        IsBusy = true;
        try
        {
            return await UserDataManager.Instance.EquipAsync(unitId, equipmentUniqueId);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<EquipmentOperationFailure> UnequipAsync(
        string unitId,
        EquipmentType equipmentType)
    {
        if (IsBusy)
            return EquipmentOperationFailure.InvalidRequest;

        IsBusy = true;
        try
        {
            return await UserDataManager.Instance.UnequipAsync(unitId, equipmentType);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<EquipmentOperationFailure> EnhanceAsync(
        string equipmentUniqueId,
        System.Collections.Generic.IReadOnlyDictionary<string, int> materials)
    {
        if (IsBusy)
            return EquipmentOperationFailure.InvalidRequest;

        IsBusy = true;
        try
        {
            return await UserDataManager.Instance.EnhanceEquipmentAsync(
                equipmentUniqueId,
                materials);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
