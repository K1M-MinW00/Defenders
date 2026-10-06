using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public sealed class LabDevelopmentTests
{
    [TestCase(1, 0)]
    [TestCase(4, 0)]
    [TestCase(5, 5)]
    [TestCase(10, 10)]
    [TestCase(20, 20)]
    public void CardCapacity_UnlocksFiveSlotsEveryFiveLevels(int level, int expected)
    {
        Assert.That(LabDevelopmentUseCase.GetCardCapacity(level), Is.EqualTo(expected));
    }

    [Test]
    public void CreateOffer_ReturnsUniqueUnownedCards()
    {
        LabConfigSO config = ScriptableObject.CreateInstance<LabConfigSO>();
        config.ChoiceCount = 4;
        for (int i = 0; i < 6; i++)
        {
            LabCardDataSO card = ScriptableObject.CreateInstance<LabCardDataSO>();
            card.CardId = $"card_{i}";
            card.DrawWeight = 1;
            config.Cards.Add(card);
        }
        UserDataRoot data = Data(level: 10, gold: 1000);
        data.Lab.AcquiredCardIds.Add("card_0");
        LabOfferResult result = new LabDevelopmentUseCase(new FakeRepository(), "u", data, config, new System.Random(1)).CreateOffer();
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Cards, Has.Count.EqualTo(4));
        Assert.That(result.Cards, Is.Unique);
        Assert.That(result.Cards.Any(c => c.CardId == "card_0"), Is.False);
    }

    [Test]
    public async Task Acquire_AtomicallySpendsAndAddsCard()
    {
        LabConfigSO config = ScriptableObject.CreateInstance<LabConfigSO>();
        config.BaseDevelopmentCost = 100;
        LabCardDataSO card = ScriptableObject.CreateInstance<LabCardDataSO>(); card.CardId = "attack"; card.DrawWeight = 1; config.Cards.Add(card);
        UserDataRoot data = Data(level: 5, gold: 150);
        FakeRepository repository = new();
        LabDevelopmentUseCase useCase = new(repository, "u", data, config);
        LabDevelopmentFailure result = await useCase.AcquireAsync(card, 100, new[] { card });
        Assert.That(result, Is.EqualTo(LabDevelopmentFailure.None));
        Assert.That(data.Resource.Gold, Is.EqualTo(50));
        Assert.That(data.Lab.AcquiredCardIds, Contains.Item("attack"));
        Assert.That(repository.SavedSections, Is.EqualTo(1));
    }

    private static UserDataRoot Data(int level, int gold) => new()
    {
        Profile = new UserProfileData { UserId = "u", Level = level },
        Resource = new UserResourceData { Gold = gold },
        Lab = new UserLabData(),
    };

    private sealed class FakeRepository : IUserDataRepository
    {
        public int SavedSections;
        public Task<UserDataLoadResult> LoadAsync(string userId) => Task.FromResult(UserDataLoadResult.NotFound());
        public Task CreateAsync(string userId, UserDataRoot data) => Task.CompletedTask;
        public Task SaveAllAsync(string userId, UserDataRoot data) => Task.CompletedTask;
        public Task SaveProfileAsync(string userId, UserProfileData profile) => Task.CompletedTask;
        public Task SaveResourcesAsync(string userId, UserResourceData resources) => Task.CompletedTask;
        public Task SaveProgressAsync(string userId, UserProgressData progress) => Task.CompletedTask;
        public Task SaveRosterAsync(string userId, UserRosterData roster) => Task.CompletedTask;
        public Task SaveSectionsAsync(string userId, UserDataUpdate update) { SavedSections++; return Task.CompletedTask; }
    }
}
