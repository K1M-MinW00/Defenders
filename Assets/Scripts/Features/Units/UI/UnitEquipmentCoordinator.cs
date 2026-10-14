using System;
using System.Threading.Tasks;

/// <summary>
/// Executes unit-equipment mutations and prevents overlapping requests.
/// </summary>
public sealed class UnitEquipmentCoordinator
{
    private readonly UserDataManager userDataManager;

    public bool IsBusy { get; private set; }

    public UnitEquipmentCoordinator(UserDataManager userDataManager)
    {
        this.userDataManager = userDataManager ?? throw new ArgumentNullException(nameof(userDataManager));
    }

    public async Task<EquipmentOperationFailure> EquipAsync(
        string unitId,
        string equipmentUniqueId)
    {
        if (IsBusy)
            return EquipmentOperationFailure.InvalidRequest;

        IsBusy = true;
        try
        {
            return await userDataManager.EquipAsync(unitId, equipmentUniqueId);
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
            return await userDataManager.UnequipAsync(unitId, equipmentType);
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
            return await userDataManager.EnhanceEquipmentAsync(
                equipmentUniqueId,
                materials);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
