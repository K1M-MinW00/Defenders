using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class GachaUseCase
{
    private readonly IUserDataRepository repository;
    private readonly IGachaRandom random;
    private readonly string userId;
    private readonly UserDataRoot userData;
    private bool isExecuting;

    public GachaUseCase(
        IUserDataRepository repository,
        string userId,
        UserDataRoot userData,
        IGachaRandom random)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentException("User ID is null or empty.", nameof(userId))
            : userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
        this.random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public async Task<RecruitUnitsResult> ExecuteAsync(RecruitUnitsCommand command)
    {
        if (isExecuting || command == null || (command.Count != 1 && command.Count != 10))
            return RecruitUnitsResult.Fail(RecruitUnitsFailure.InvalidRequest);

        GachaDataSO banner = command.Banner;

        if (!IsValidBanner(banner) || userData.Resource == null || userData.Inventory == null ||
            userData.Gacha == null || userData.Roster == null)
        {
            return RecruitUnitsResult.Fail(RecruitUnitsFailure.InvalidBanner);
        }

        InventoryStackItem currentTickets = userData.Inventory.Consumables?
            .FirstOrDefault(item => item != null && item.ItemId == banner.ticketItemId);
        int ticketUseCount = Math.Min(Math.Max(currentTickets?.Count ?? 0, 0), command.Count);
        int gemRecruitCount = command.Count - ticketUseCount;
        long gemCost = (long)gemRecruitCount * banner.gemCost;

        if (gemCost > int.MaxValue || userData.Resource.Gem < gemCost)
            return RecruitUnitsResult.Fail(RecruitUnitsFailure.InsufficientCurrency);

        UserResourceData nextResources = UserDataCloner.Copy(userData.Resource);
        UserInventoryData nextInventory = UserDataCloner.Copy(userData.Inventory);
        UserGachaData nextGacha = UserDataCloner.Copy(userData.Gacha);
        UserRosterData nextRoster = UserDataCloner.Copy(userData.Roster);

        ConsumeTickets(nextInventory, banner.ticketItemId, ticketUseCount);
        nextResources.Gem -= (int)gemCost;

        List<GachaResult> results = new(command.Count);
        long duplicateReward = 0;

        for (int i = 0; i < command.Count; i++)
        {
            Rarity rarity = IsLegendGuaranteed(nextGacha, banner)
                ? Rarity.Legend
                : RollRarity(banner);
            UnitDataSO unit = RollUnit(banner, rarity);

            if (unit == null || string.IsNullOrWhiteSpace(unit.unitId))
                return RecruitUnitsResult.Fail(RecruitUnitsFailure.EmptyPool);

            UpdatePity(nextGacha, banner.recruitType, rarity);

            GachaResult result = new()
            {
                Unit = unit,
                IsLegend = rarity == Rarity.Legend,
            };

            GiveUnit(nextRoster, unit, result, ref duplicateReward);
            results.Add(result);
        }

        if (duplicateReward > (long)int.MaxValue - nextResources.Gem)
            return RecruitUnitsResult.Fail(RecruitUnitsFailure.InvalidRequest);

        nextResources.Gem += (int)duplicateReward;
        isExecuting = true;

        try
        {
            await repository.SaveSectionsAsync(userId, new UserDataUpdate
            {
                Resources = nextResources,
                Inventory = nextInventory,
                Gacha = nextGacha,
                Roster = nextRoster,
            });
        }
        catch
        {
            return RecruitUnitsResult.Fail(RecruitUnitsFailure.SaveFailed);
        }
        finally
        {
            isExecuting = false;
        }

        userData.Resource = nextResources;
        userData.Inventory = nextInventory;
        userData.Gacha = nextGacha;
        userData.Roster = nextRoster;

        return RecruitUnitsResult.Success(results);
    }

    private static bool IsValidBanner(GachaDataSO banner)
    {
        if (banner == null || string.IsNullOrWhiteSpace(banner.ticketItemId) ||
            banner.gemCost < 0 || banner.legendPityCount <= 0 ||
            banner.normalRate < 0f || banner.rareRate < 0f || banner.legendRate < 0f)
        {
            return false;
        }

        ItemDataSO ticket = ItemDatabase.Get(banner.ticketItemId);

        if (ticket == null || ticket.Category != ItemCategory.Consumable ||
            banner.legendPool == null || banner.legendPool.Count == 0 ||
            (banner.normalRate > 0f && (banner.normalPool == null || banner.normalPool.Count == 0)) ||
            (banner.rareRate > 0f && (banner.rarePool == null || banner.rarePool.Count == 0)))
        {
            return false;
        }

        float rateTotal = banner.normalRate + banner.rareRate + banner.legendRate;
        return Math.Abs(rateTotal - 100f) <= 0.01f;
    }

    private static void ConsumeTickets(UserInventoryData inventory, string ticketId, int count)
    {
        if (count <= 0)
            return;

        InventoryStackItem tickets = inventory.Consumables
            .First(item => item != null && item.ItemId == ticketId);
        tickets.Count -= count;

        if (tickets.Count == 0)
            inventory.Consumables.Remove(tickets);
    }

    private bool IsLegendGuaranteed(UserGachaData gacha, GachaDataSO banner)
    {
        return GetPity(gacha, banner.recruitType) >= banner.legendPityCount - 1;
    }

    private Rarity RollRarity(GachaDataSO banner)
    {
        float roll = random.Range(0f, 100f);

        if (roll < banner.legendRate)
            return Rarity.Legend;

        roll -= banner.legendRate;

        return roll < banner.rareRate ? Rarity.Rare : Rarity.Normal;
    }

    private UnitDataSO RollUnit(GachaDataSO banner, Rarity rarity)
    {
        List<UnitDataSO> pool = rarity switch
        {
            Rarity.Normal => banner.normalPool,
            Rarity.Rare => banner.rarePool,
            Rarity.Legend => banner.legendPool,
            _ => null,
        };

        if (pool == null || pool.Count == 0)
            return null;

        return pool[random.Range(0, pool.Count)];
    }

    private static int GetPity(UserGachaData gacha, RecruitType recruitType)
    {
        return recruitType == RecruitType.Special ? gacha.SpecialPity : gacha.NormalPity;
    }

    private static void UpdatePity(UserGachaData gacha, RecruitType recruitType, Rarity rarity)
    {
        int nextPity = rarity == Rarity.Legend ? 0 : GetPity(gacha, recruitType) + 1;

        if (recruitType == RecruitType.Special)
            gacha.SpecialPity = nextPity;
        else
            gacha.NormalPity = nextPity;
    }

    private static void GiveUnit(
        UserRosterData roster,
        UnitDataSO unit,
        GachaResult result,
        ref long duplicateReward)
    {
        UserUnitData ownedUnit = roster.OwnedUnits
            .FirstOrDefault(owned => owned != null && owned.UnitId == unit.unitId);

        if (ownedUnit == null)
        {
            roster.OwnedUnits.Add(new UserUnitData { UnitId = unit.unitId, Level = 1 });
            return;
        }

        if (ownedUnit.LimitBreak + ownedUnit.DuplicateCount < UnitLimitBreakUseCase.MaxLimitBreak)
        {
            ownedUnit.DuplicateCount++;
            return;
        }

        result.IsDuplicateReward = true;
        duplicateReward += GetDuplicateReward(unit.rarity);
    }

    private static int GetDuplicateReward(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Normal => 30,
            Rarity.Rare => 100,
            Rarity.Legend => 300,
            _ => 0,
        };
    }
}
