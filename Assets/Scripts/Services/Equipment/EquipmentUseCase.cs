using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public enum EquipmentOperationFailure { None, InvalidRequest, ValidationFailed, SaveFailed }

public sealed class EquipmentUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;
    private readonly EquipmentLoadoutService loadout;
    private readonly EquipmentEnhancementService enhancement;
    private readonly EquipmentDismantleService dismantle;
    private bool isExecuting;

    public EquipmentUseCase(IUserDataRepository repository, string userId, UserDataRoot userData)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
        loadout = new EquipmentLoadoutService(GameConfig.Items);
        enhancement = new EquipmentEnhancementService(GameConfig.Items, GameConfig.EquipmentBalance, new UnityEquipmentRandom());
        dismantle = new EquipmentDismantleService(GameConfig.Items, GameConfig.EquipmentBalance);
    }

    public Task<EquipmentOperationFailure> EquipAsync(string unitId, string equipmentId) => MutateAsync((inventory, roster) =>
        loadout.Equip(inventory, roster, unitId, equipmentId).Succeeded);

    public Task<EquipmentOperationFailure> UnequipAsync(string unitId, EquipmentType type) => MutateAsync((inventory, roster) =>
        loadout.Unequip(inventory, roster, unitId, type).Succeeded);

    public Task<EquipmentOperationFailure> EnhanceAsync(string equipmentId, IReadOnlyDictionary<string, int> materials) => MutateAsync((inventory, roster) =>
        enhancement.Enhance(inventory, equipmentId, materials).Succeeded);

    public Task<EquipmentOperationFailure> DismantleAsync(IReadOnlyCollection<string> equipmentIds) => MutateAsync((inventory, roster) =>
        dismantle.Dismantle(inventory, roster, equipmentIds).Succeeded);

    private async Task<EquipmentOperationFailure> MutateAsync(Func<UserInventoryData, UserRosterData, bool> mutation)
    {
        if (isExecuting || mutation == null || string.IsNullOrWhiteSpace(userId) || userData.Inventory == null || userData.Roster == null)
            return EquipmentOperationFailure.InvalidRequest;
        UserInventoryData nextInventory = UserDataCloner.Copy(userData.Inventory);
        UserRosterData nextRoster = UserDataCloner.Copy(userData.Roster);
        if (!mutation(nextInventory, nextRoster)) return EquipmentOperationFailure.ValidationFailed;
        nextRoster.Power = FormationPowerCalculator.Calculate(nextRoster, GameConfig.Units, nextInventory);
        isExecuting = true;
        try
        {
            await repository.SaveSectionsAsync(userId, new UserDataUpdate { Inventory = nextInventory, Roster = nextRoster });
        }
        catch { return EquipmentOperationFailure.SaveFailed; }
        finally { isExecuting = false; }
        userData.Inventory = nextInventory;
        userData.Roster = nextRoster;
        return EquipmentOperationFailure.None;
    }
}
