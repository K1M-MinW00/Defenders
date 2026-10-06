using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class LabDevelopmentUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;
    private readonly LabConfigSO config;
    private readonly Random random;

    public LabDevelopmentUseCase(IUserDataRepository repository, string userId, UserDataRoot userData, LabConfigSO config, Random random = null)
    {
        this.repository = repository;
        this.userId = userId;
        this.userData = userData;
        this.config = config;
        this.random = random ?? new Random();
    }

    public static int GetCardCapacity(int accountLevel) => Math.Max(0, accountLevel / 5 * 5);
    public int GetNextCost() => config?.GetCost(userData?.Lab?.AcquiredCardIds?.Count ?? 0) ?? 0;

    public LabOfferResult CreateOffer()
    {
        if (userData?.Profile == null || userData.Resource == null || userData.Lab == null || config == null)
            return LabOfferResult.Fail(LabDevelopmentFailure.InvalidRequest);

        int owned = userData.Lab.AcquiredCardIds.Count;
        if (owned >= GetCardCapacity(userData.Profile.Level))
            return LabOfferResult.Fail(LabDevelopmentFailure.LevelLocked);

        int cost = GetNextCost();
        if (userData.Resource.ResearchMaterial < cost)
            return LabOfferResult.Fail(LabDevelopmentFailure.InsufficientCurrency);

        HashSet<string> acquired = new(userData.Lab.AcquiredCardIds);
        List<LabCardDataSO> pool = config.Cards.Where(card => card != null && !acquired.Contains(card.CardId)).ToList();
        if (pool.Count == 0)
            return LabOfferResult.Fail(LabDevelopmentFailure.AllCardsAcquired);

        List<LabCardDataSO> choices = new();
        while (choices.Count < config.ChoiceCount && pool.Count > 0)
        {
            int index = PickWeightedIndex(pool);
            choices.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return LabOfferResult.Success(choices, cost);
    }

    public async Task<LabDevelopmentFailure> AcquireAsync(LabCardDataSO selected, int quotedCost, IReadOnlyList<LabCardDataSO> offeredCards)
    {
        if (selected == null || offeredCards == null || !offeredCards.Contains(selected) || quotedCost != GetNextCost())
            return LabDevelopmentFailure.InvalidRequest;
        if (userData.Lab.AcquiredCardIds.Contains(selected.CardId))
            return LabDevelopmentFailure.InvalidRequest;
        if (userData.Lab.AcquiredCardIds.Count >= GetCardCapacity(userData.Profile.Level))
            return LabDevelopmentFailure.LevelLocked;
        if (userData.Resource.ResearchMaterial < quotedCost)
            return LabDevelopmentFailure.InsufficientCurrency;

        UserResourceData nextResources = UserDataCloner.Copy(userData.Resource);
        UserLabData nextLab = UserDataCloner.Copy(userData.Lab);
        nextResources.ResearchMaterial -= quotedCost;
        nextLab.AcquiredCardIds.Add(selected.CardId);

        try
        {
            await repository.SaveSectionsAsync(userId, new UserDataUpdate { Resources = nextResources, Lab = nextLab });
        }
        catch
        {
            return LabDevelopmentFailure.SaveFailed;
        }

        userData.Resource = nextResources;
        userData.Lab = nextLab;
        return LabDevelopmentFailure.None;
    }

    private int PickWeightedIndex(IReadOnlyList<LabCardDataSO> pool)
    {
        double total = pool.Sum(card => Math.Max(0d, card.GetEffectiveDrawWeight()));
        if (total <= 0d)
            return random.Next(pool.Count);

        double roll = random.NextDouble() * total;
        for (int i = 0; i < pool.Count; i++)
        {
            roll -= Math.Max(0d, pool[i].GetEffectiveDrawWeight());
            if (roll <= 0d)
                return i;
        }
        return pool.Count - 1;
    }
}
