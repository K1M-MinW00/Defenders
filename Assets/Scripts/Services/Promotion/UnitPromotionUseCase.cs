using System;
using System.Linq;
using System.Threading.Tasks;

public sealed class UnitPromotionUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;
    private bool isExecuting;

    public UnitPromotionUseCase(IUserDataRepository repository, string userId, UserDataRoot userData)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentException("User ID is null or empty.", nameof(userId))
            : userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
    }

    public async Task<PromoteUnitResult> ExecuteAsync(PromoteUnitCommand command)
    {
        if (isExecuting || command == null || string.IsNullOrWhiteSpace(command.UnitId))
            return PromoteUnitResult.Fail(PromoteUnitFailure.InvalidRequest);

        UnitDataSO unitDefinition = GameConfig.Units.Get(command.UnitId);
        UserUnitData currentUnit = userData.Roster?.OwnedUnits?
            .FirstOrDefault(unit => unit != null && unit.UnitId == command.UnitId);

        if (unitDefinition == null || currentUnit == null)
            return PromoteUnitResult.Fail(PromoteUnitFailure.UnitNotFound);

        PromotionCost[] costs = unitDefinition.promotionCost;

        if (costs == null || currentUnit.Promotion >= costs.Length)
            return PromoteUnitResult.Fail(PromoteUnitFailure.MaxPromotion);

        PromotionCost cost = costs[currentUnit.Promotion];
        MaterialDataSO material = cost == null ? null : GameConfig.Items.Get(cost.MaterialId) as MaterialDataSO;

        if (cost == null || string.IsNullOrWhiteSpace(cost.MaterialId) || cost.Count <= 0 ||
            material == null || material.MaterialType != MaterialType.Promotion)
        {
            return PromoteUnitResult.Fail(PromoteUnitFailure.InvalidCost);
        }

        UserInventoryData nextInventory = UserDataCloner.Copy(userData.Inventory);
        UserRosterData nextRoster = UserDataCloner.Copy(userData.Roster);
        InventoryMutationService inventoryMutations = new(GameConfig.Items);
        if (!inventoryMutations.Consume(nextInventory, cost.MaterialId, cost.Count).Succeeded)
            return PromoteUnitResult.Fail(PromoteUnitFailure.InsufficientMaterials);

        UserUnitData nextUnit = nextRoster.OwnedUnits
            .First(unit => unit != null && unit.UnitId == command.UnitId);
        nextUnit.Promotion++;
        nextRoster.Power = FormationPowerCalculator.Calculate(nextRoster, GameConfig.Units);

        isExecuting = true;

        try
        {
            await repository.SaveSectionsAsync(userId, new UserDataUpdate
            {
                Inventory = nextInventory,
                Roster = nextRoster,
            });
        }
        catch
        {
            return PromoteUnitResult.Fail(PromoteUnitFailure.SaveFailed);
        }
        finally
        {
            isExecuting = false;
        }

        userData.Inventory = nextInventory;
        userData.Roster = nextRoster;

        return PromoteUnitResult.Success(nextUnit.Promotion);
    }
}
