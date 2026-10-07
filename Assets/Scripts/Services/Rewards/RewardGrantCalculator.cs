using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Firestore;

public static class RewardGrantCalculator
{
    public static RewardGrantResult Calculate(
        UserDataRoot source,
        IReadOnlyCollection<RewardData> rewards)
    {
        if (source?.Profile == null || source.Resource == null || source.Inventory == null || source.Roster == null)
            return RewardGrantResult.Fail(RewardGrantFailure.InvalidData);

        if (rewards == null || rewards.Count == 0)
            return RewardGrantResult.Fail(RewardGrantFailure.InvalidReward);

        UserResourceData resources = UserDataCloner.Copy(source.Resource);
        UserInventoryData inventory = UserDataCloner.Copy(source.Inventory);
        UserRosterData roster = UserDataCloner.Copy(source.Roster);
        UserProfileData profile = UserDataCloner.Copy(source.Profile);
        InventoryMutationService inventoryMutations = new(GameConfig.Items);

        try
        {
            foreach (RewardData reward in rewards)
            {
                if (reward == null || reward.Amount <= 0 ||
                    !ApplyReward(reward, resources, inventory, roster, profile, inventoryMutations))
                    return RewardGrantResult.Fail(RewardGrantFailure.InvalidReward);
            }
        }
        catch (OverflowException)
        {
            return RewardGrantResult.Fail(RewardGrantFailure.Overflow);
        }

        return RewardGrantResult.Success(resources, inventory, roster, profile);
    }

    private static bool ApplyReward(
        RewardData reward,
        UserResourceData resources,
        UserInventoryData inventory,
        UserRosterData roster,
        UserProfileData profile,
        InventoryMutationService inventoryMutations)
    {
        switch (reward.Type)
        {
            case RewardType.Gold:
                resources.Gold = checked(resources.Gold + reward.Amount);
                return true;

            case RewardType.ResearchMaterial:
                resources.ResearchMaterial = checked(resources.ResearchMaterial + reward.Amount);
                return true;

            case RewardType.Gem:
                resources.Gem = checked(resources.Gem + reward.Amount);
                return true;

            case RewardType.Fuel:
                resources.Fuel = checked(resources.Fuel + reward.Amount);

                if (resources.Fuel >= resources.MaxFuel)
                    resources.LastFuelUpdateTime = Timestamp.GetCurrentTimestamp();

                return true;

            case RewardType.Item:
                ItemDataSO stackItem = GameConfig.Items.Get(reward.Id);
                return stackItem != null && stackItem.Category != ItemCategory.Equipment &&
                    inventoryMutations.Add(inventory, reward.Id, reward.Amount).Succeeded;

            case RewardType.Equipment:
                ItemDataSO equipment = GameConfig.Items.Get(reward.Id);
                return equipment?.Category == ItemCategory.Equipment &&
                    inventoryMutations.Add(inventory, reward.Id, reward.Amount).Succeeded;

            case RewardType.Unit:
                return AddUnit(resources, roster, reward.Id, reward.Amount);

            case RewardType.Experience:
                return AddExperience(profile, reward.Amount);

            default:
                return false;
        }
    }

    private static bool AddExperience(UserProfileData profile, int amount)
    {
        UserLevelProgressionSO progression = GameConfig.UserLevelProgression;
        if (profile == null || progression == null || profile.Level < 1)
            return false;

        profile.Exp = checked(profile.Exp + amount);

        while (true)
        {
            int requiredExp = progression.GetRequiredExp(profile.Level);
            if (requiredExp <= 0 || profile.Exp < requiredExp)
                return true;

            profile.Exp -= requiredExp;
            profile.Level = checked(profile.Level + 1);
        }
    }

    private static bool AddUnit(
        UserResourceData resources,
        UserRosterData roster,
        string unitId,
        int amount)
    {
        UnitDataSO unit = GameConfig.Units.Get(unitId);

        if (unit == null)
            return false;

        for (int i = 0; i < amount; i++)
        {
            UserUnitData owned = roster.OwnedUnits
                .FirstOrDefault(candidate => candidate != null && candidate.UnitId == unitId);

            if (owned == null)
            {
                roster.OwnedUnits.Add(new UserUnitData { UnitId = unitId, Level = 1 });
            }
            else if (owned.LimitBreak + owned.DuplicateCount < UnitLimitBreakUseCase.MaxLimitBreak)
            {
                owned.DuplicateCount++;
            }
            else
            {
                GachaEconomyConfigSO economyConfig = GameConfig.GachaEconomy;
                if (economyConfig == null)
                    return false;

                resources.Gem = checked(resources.Gem + economyConfig.GetDuplicateGemReward(unit.rarity));
            }
        }

        return true;
    }

}
