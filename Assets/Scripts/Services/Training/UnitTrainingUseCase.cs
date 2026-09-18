using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class UnitTrainingUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;
    private bool isExecuting;

    public UnitTrainingUseCase(IUserDataRepository repository, string userId, UserDataRoot userData)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentException("User ID is null or empty.", nameof(userId))
            : userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
    }

    public async Task<TrainUnitResult> ExecuteAsync(TrainUnitCommand command)
    {
        if (isExecuting || command == null || string.IsNullOrWhiteSpace(command.UnitId) ||
            command.Materials == null || command.Materials.Count == 0)
        {
            return TrainUnitResult.Fail(TrainUnitFailure.InvalidRequest);
        }

        UnitDataSO unitDefinition = UnitDatabase.Get(command.UnitId);
        UserUnitData currentUnit = userData.Roster?.OwnedUnits?
            .FirstOrDefault(unit => unit != null && unit.UnitId == command.UnitId);

        if (unitDefinition == null || currentUnit == null)
            return TrainUnitResult.Fail(TrainUnitFailure.UnitNotFound);

        if (currentUnit.Level >= unitDefinition.maxLevel)
            return TrainUnitResult.Fail(TrainUnitFailure.MaxLevel);

        if (!TryCalculateCost(command.Materials, out int gainedExp, out int goldCost))
            return TrainUnitResult.Fail(TrainUnitFailure.InvalidRequest);

        if (userData.Resource == null || userData.Resource.Gold < goldCost)
            return TrainUnitResult.Fail(TrainUnitFailure.InsufficientGold);

        UserResourceData nextResources = UserDataCloner.Copy(userData.Resource);
        UserInventoryData nextInventory = UserDataCloner.Copy(userData.Inventory);
        UserRosterData nextRoster = UserDataCloner.Copy(userData.Roster);

        if (!TryConsumeMaterials(nextInventory, command.Materials))
            return TrainUnitResult.Fail(TrainUnitFailure.InsufficientMaterials);

        UserUnitData nextUnit = nextRoster.OwnedUnits
            .First(unit => unit != null && unit.UnitId == command.UnitId);

        nextResources.Gold -= goldCost;
        ApplyExp(nextUnit, gainedExp, unitDefinition.maxLevel);

        isExecuting = true;

        try
        {
            await repository.SaveSectionsAsync(userId, new UserDataUpdate
            {
                Resources = nextResources,
                Inventory = nextInventory,
                Roster = nextRoster,
            });
        }
        catch
        {
            return TrainUnitResult.Fail(TrainUnitFailure.SaveFailed);
        }
        finally
        {
            isExecuting = false;
        }

        userData.Resource = nextResources;
        userData.Inventory = nextInventory;
        userData.Roster = nextRoster;

        return TrainUnitResult.Success(nextUnit.Level, nextUnit.Exp);
    }

    private static bool TryCalculateCost(
        IReadOnlyDictionary<string, int> materials,
        out int gainedExp,
        out int goldCost)
    {
        long totalExp = 0;

        foreach (KeyValuePair<string, int> pair in materials)
        {
            if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value <= 0)
            {
                gainedExp = 0;
                goldCost = 0;
                return false;
            }

            MaterialDataSO material = ItemDatabase.Get(pair.Key) as MaterialDataSO;

            if (material == null || material.MaterialType != MaterialType.Training || material.Value <= 0)
            {
                gainedExp = 0;
                goldCost = 0;
                return false;
            }

            totalExp += (long)material.Value * pair.Value;

            if (totalExp > int.MaxValue)
            {
                gainedExp = 0;
                goldCost = 0;
                return false;
            }
        }

        gainedExp = (int)totalExp;
        goldCost = gainedExp;
        return gainedExp > 0;
    }

    private static bool TryConsumeMaterials(
        UserInventoryData inventory,
        IReadOnlyDictionary<string, int> materials)
    {
        if (inventory?.Materials == null)
            return false;

        foreach (KeyValuePair<string, int> pair in materials)
        {
            InventoryStackItem item = inventory.Materials.FirstOrDefault(x => x != null && x.ItemId == pair.Key);

            if (item == null || item.Count < pair.Value)
                return false;
        }

        foreach (KeyValuePair<string, int> pair in materials)
        {
            InventoryStackItem item = inventory.Materials.First(x => x != null && x.ItemId == pair.Key);
            item.Count -= pair.Value;

            if (item.Count == 0)
                inventory.Materials.Remove(item);
        }

        return true;
    }

    private static void ApplyExp(UserUnitData unit, int amount, int maxLevel)
    {
        unit.Exp += amount;

        while (unit.Level < maxLevel)
        {
            int requiredExp = UnitExpTable.GetRequiredExp(unit.Level);

            if (unit.Exp < requiredExp)
                break;

            unit.Exp -= requiredExp;
            unit.Level++;
        }

        if (unit.Level >= maxLevel)
        {
            unit.Level = maxLevel;
            unit.Exp = 0;
        }
    }

}
